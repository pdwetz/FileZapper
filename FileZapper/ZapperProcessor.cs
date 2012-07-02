/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2012 Peter Wetzel

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace FileZapper
{
    class ZapperProcessor
    {
        /// <summary>
        /// Extensions we may want to ignore, such as .readme or somesuch.
        /// </summary>
        internal string[] SkippedExtensions { get; private set; }

        /// <summary>
        /// Extensions we want to delete if we find them
        /// </summary>
        internal string[] UnwantedExtensions { get; private set; }

        /// <summary>
        /// Folders we want to delete if we find them
        /// </summary>
        internal string[] UnwantedFolders { get; private set; }

        /// <summary>
        /// Root folders we'll be recursively processing
        /// </summary>
        internal IEnumerable<ZapperFolder> EnabledRootFolders { get; private set; }

        internal string Connection;
        internal Guid Id { get; set; }
        internal int CurrentPhase { get; private set; }

        public ZapperProcessor()
        {
            Id = Guid.NewGuid();
            Connection = ConfigurationManager.ConnectionStrings["zapper"].ConnectionString;
        }

        /// <summary>
        /// Core engine; once user kicks it off, will run until completed.
        /// </summary>
        public void Process()
        {
            try
            {
                var extensions = GetExtensionsAll();
                SkippedExtensions = extensions.Where(x => x.IsIgnored).Select(z => z.Name).ToArray();
                UnwantedExtensions = extensions.Where(x => x.IsUnwanted).Select(z => z.Name).ToArray();
                UnwantedFolders = new string[] { "__MACOSX" };
                EnabledRootFolders = GetFoldersEnabled();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("FileZapper   Copyright (C) 2012 Peter Wetzel");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY; for details see license.txt.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Current configuration settings...");
                WriteSetting("Ignored extensions", string.Join(", ", SkippedExtensions));
                WriteSetting("Unwanted extensions", string.Join(", ", UnwantedExtensions));
                WriteSetting("Unwanted folders", string.Join(", ", UnwantedFolders));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Enabled folders (score - path):");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var f in EnabledRootFolders)
                {
                    Console.WriteLine("{0} - {1}", f.Priority, f.FullPath);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Please choose a number for a starting phase.");
                List<IZapperPhase> allphases = new List<IZapperPhase>();
                allphases.Add(new PhaseCleanup { PhaseOrder = 1, ZapperProcessor = this, IsInitialPhase = true });
                allphases.Add(new PhaseParseFilesystem { PhaseOrder = 2, ZapperProcessor = this });
                allphases.Add(new PhaseCalculateHashes { PhaseOrder = 3, ZapperProcessor = this });
                allphases.Add(new PhaseRemoveDuplicates { PhaseOrder = 4, ZapperProcessor = this });
                var orderedphases = allphases.OrderBy(x => x.PhaseOrder);
                foreach (var phase in orderedphases)
                {
                    Console.WriteLine("{0}) {1}{2}", phase.PhaseOrder, phase.Name, phase.IsInitialPhase ? " [default]" : "");
                }
                Console.Write("Phase> ");
                string sChoice = Console.ReadLine();
                int iInitialPhase = string.IsNullOrWhiteSpace(sChoice)
                    ? allphases.First(x => x.IsInitialPhase).PhaseOrder : Convert.ToInt32(sChoice);
                CurrentPhase = iInitialPhase;
                try
                {
                    StartSession();
                    var phases = orderedphases.Where(x => x.PhaseOrder >= iInitialPhase);
                    foreach (var phase in phases)
                    {
                        WritePhase(phase.Name);
                        phase.Process();
                        CurrentPhase++;
                    }
                }
                catch (Exception ex)
                {
                    Exceptioneer.Log(ex);
                }
                finally
                {
                    EndSession();
                }
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(ex);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}: Done. Press any key to continue.", DateTime.Now.ToString("HH:mm:ss.fff"));
            Console.ReadLine();
        }

        /// <summary>
        /// Kick off a new session; used for tracking overall time running and delete history.
        /// </summary>
        private void StartSession()
        {
            using (SqlConnection con = new SqlConnection(Connection))
            {
                con.Open();
                string sSql = "insert zsession (SessionId, StartDate) values (@SessionId, SYSDATETIME())";
                con.Execute(sSql, new { @SessionId = Id });
            }
        }

        /// <summary>
        /// End session with current timestamp
        /// </summary>
        private void EndSession()
        {
            using (SqlConnection con = new SqlConnection(Connection))
            {
                con.Open();
                string sSql = "update zsession set enddate = SYSDATETIME() where SessionId = @SessionId";
                con.Execute(sSql, new { @SessionId = Id });
            }
        }

        /// <summary>
        /// Helper console method for writing setting name/value pair
        /// </summary>
        private void WriteSetting(string sName, string sValue)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0}: ", sName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(sValue);
        }

        /// <summary>
        /// Helper console method for writing new processor phase message
        /// </summary>
        private void WritePhase(string sMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0}: Phase {1} - {2}", DateTime.Now.ToString("HH:mm:ss.fff"), CurrentPhase, sMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Retrieve all explicitly managed extensions from the database; note that these are exceptions, by default
        /// we allow all extensions
        /// </summary>
        private IEnumerable<ZapperExtension> GetExtensionsAll()
        {
            using (var con = new SqlConnection(Connection))
            {
                con.Open();
                string sSql = @"select * from zextension";
                return con.Query<ZapperExtension>(sSql);
            }
        }

        /// <summary>
        /// Retrieve all explicitly tracked folders that are enabled; they may or may not have been processed before.
        /// </summary>
        private IEnumerable<ZapperFolder> GetFoldersEnabled()
        {
            using (var con = new SqlConnection(Connection))
            {
                con.Open();
                string sSql = @"select * from zfolder where isenabled = 1";
                return con.Query<ZapperFolder>(sSql);
            }
        }

        /// <summary>
        /// Retrieve all files from database
        /// </summary>
        internal IEnumerable<ZapperFile> GetFilesAll()
        {
            using (var con = new SqlConnection(Connection))
            {
                con.Open();
                string sSql = @"select * from zfile";
                return con.Query<ZapperFile>(sSql);
            }
        }
    }
}
