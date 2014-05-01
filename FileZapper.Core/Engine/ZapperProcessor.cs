/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2014 Peter Wetzel

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using FileZapper.Core.Configuration;
using FileZapper.Core.Data;
using FileZapper.Core.Utilities;
using log4net;
using System.IO;
using System.Reflection;
using CsvHelper;

namespace FileZapper.Core.Engine
{
    public class ZapperProcessor
    {
        private readonly ILog _log = LogManager.GetLogger("ZapperProcessor");

        public ConcurrentDictionary<string, ZapperFile> ZapperFiles { get; set; }
        public ConcurrentDictionary<string, ZapperFileDeleted> ZapperFilesDeleted { get; set; }

        private readonly ZapperSession _zapperSession = new ZapperSession { Id = Guid.NewGuid() };
        public ZapperSession ZapperSession { get { return _zapperSession; } }

        private readonly IOrderedEnumerable<IZapperPhase> _phases;
        public FileZapperSettings Settings { get; set; }

        public ZapperProcessor(FileZapperSettings settings = null, IList<IZapperPhase> phases = null)
        {
            ZapperFiles = new ConcurrentDictionary<string, ZapperFile>();
            ZapperFilesDeleted = new ConcurrentDictionary<string, ZapperFileDeleted>();

            if (settings == null)
            {
                settings = new FileZapperSettings();
                settings.Load();
            }
            Settings = settings;

            if (phases != null)
            {
                foreach (var phase in phases) { phase.ZapperProcessor = this; }
                _phases = phases.OrderBy(x => x.PhaseOrder);
            }
            else
            {
                List<IZapperPhase> allphases = new List<IZapperPhase>();
                allphases.Add(new PhaseCleanup { PhaseOrder = 1, ZapperProcessor = this, IsInitialPhase = true });
                allphases.Add(new PhaseParseFilesystem { PhaseOrder = 2, ZapperProcessor = this });
                allphases.Add(new PhaseCalculateHashes { PhaseOrder = 3, ZapperProcessor = this });
                allphases.Add(new PhaseRemoveDuplicates { PhaseOrder = 4, ZapperProcessor = this });
                _phases = allphases.OrderBy(x => x.PhaseOrder);
            }
        }

        public void StartConsole()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("FileZapper   Copyright (C) 2014 Peter Wetzel");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY; for details see license.txt.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Current configuration settings...");
                WriteSetting("Ignored extensions", string.Join(", ", Settings.SkippedExtensions));
                WriteSetting("Unwanted extensions", string.Join(", ", Settings.UnwantedExtensions));
                WriteSetting("Unwanted folders", string.Join(", ", Settings.UnwantedFolders));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Enabled folders (score - path):");
                Console.ForegroundColor = ConsoleColor.White;
                foreach (var f in Settings.RootFolders)
                {
                    Console.WriteLine("{0} - {1}", f.Priority, f.FullPath);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Please choose a number for a starting phase.");
                foreach (var phase in _phases)
                {
                    Console.WriteLine("{0}) {1}{2}", phase.PhaseOrder, phase.Name, phase.IsInitialPhase ? " [default]" : "");
                }
                Console.Write("Phase> ");
                string sChoice = Console.ReadLine();
                int iInitialPhase = string.IsNullOrWhiteSpace(sChoice) ? _phases.First(x => x.IsInitialPhase).PhaseOrder : Convert.ToInt32(sChoice);
                _zapperSession.CurrentPhase = iInitialPhase;
                Process();
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(_log, ex);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}: Done. Press any key to continue.", DateTime.Now.ToString("HH:mm:ss.fff"));
            Console.ReadLine();
        }

        public void Process()
        {
            try
            {
                _zapperSession.StartDate = DateTime.Now;
                var phases = _phases.Where(x => x.PhaseOrder >= _zapperSession.CurrentPhase);
                foreach (var phase in phases)
                {
                    WritePhase(phase.Name);
                    phase.Process();
                    _zapperSession.PhasesProcessed++;
                    _zapperSession.CurrentPhase++;
                }
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(_log, ex);
            }
            finally
            {
                _zapperSession.EndDate = DateTime.Now;
                _zapperSession.RuntimeMS = (_zapperSession.EndDate.Value - _zapperSession.StartDate).TotalMilliseconds;
                _zapperSession.FilesAdded = ZapperFiles.Count;
                _zapperSession.FilesHashed = ZapperFiles.Values.Count(x => !string.IsNullOrWhiteSpace(x.ContentHash));
                _zapperSession.FilesDeleted = ZapperFilesDeleted.Count;
                _zapperSession.TotalFilesProcessed = _zapperSession.FilesAdded + _zapperSession.FilesDeleted;
            }
            LogResults();
        }

        public string LogResults()
        {
            string sPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Results");
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            string sFilePath = Path.Combine(sPath, "zappersessions.csv");
            bool bSessionFileExists = File.Exists(sFilePath);
            using (var textWriter = File.AppendText(sFilePath))
            {
                using (var writer = new CsvWriter(textWriter))
                {
                    if (!bSessionFileExists)
                    {
                        writer.WriteHeader<ZapperSession>();
                    }
                    writer.WriteRecord(_zapperSession);
                }
            }
            if (ZapperFiles.Count > 0)
            {
                sFilePath = Path.Combine(sPath, string.Format("files-{0}.csv", _zapperSession.Id));
                using (var csv = new CsvWriter(new StreamWriter(sFilePath)))
                {
                    var records = (IEnumerable)ZapperFiles.Values;
                    csv.WriteRecords(records);
                }
            }
            if (ZapperFilesDeleted.Count > 0)
            {
                sFilePath = Path.Combine(sPath, string.Format("deleted-{0}.csv", _zapperSession.Id));
                using (var csv = new CsvWriter(new StreamWriter(sFilePath)))
                {
                    var records = (IEnumerable)ZapperFilesDeleted.Values;
                    csv.WriteRecords(records);
                }
            }
            return sPath;
        }

        private void WriteSetting(string sName, string sValue)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0}: ", sName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(sValue);
        }

        private void WritePhase(string sMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0}: Phase {1} - {2}", DateTime.Now.ToString("HH:mm:ss.fff"), _zapperSession.CurrentPhase, sMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}