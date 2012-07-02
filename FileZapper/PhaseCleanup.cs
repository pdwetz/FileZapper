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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualBasic.FileIO;

namespace FileZapper
{
    /// <summary>
    /// Clean up the database a bit by removing missing folders and files, as well as unwanted extensions.
    /// Meant to be run prior to anything else to avoid stale data in the database.
    /// </summary>
    class PhaseCleanup : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseCleanup()
        {
            Name = "Clean up";
        }

        public void Process()
        {
            Console.WriteLine("{0}: Deleting unwanted directories from file system...", DateTime.Now.ToString("HH:mm:ss.fff"));
            foreach (var root in ZapperProcessor.EnabledRootFolders)
            {
                var dirs = Directory.EnumerateDirectories(root.FullPath, "*.*", System.IO.SearchOption.AllDirectories);
                try
                {
                    Parallel.ForEach(dirs, dir =>
                    {
                        // Exists check in case another thread has already deleted this via a different level
                        if (Directory.Exists(dir))
                        {
                            DirectoryInfo d = new DirectoryInfo(dir);
                            if (ZapperProcessor.UnwantedFolders.Contains(d.Name))
                            {
                                FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                    });
                }
                catch (AggregateException ae)
                {
                    ae.Handle(e =>
                    {
                        Exceptioneer.Log(e);
                        return true;
                    });
                }
            }

            var directories = GetFileDirectories();
            Console.WriteLine("{0}: Deleting non-existent directories from database...", DateTime.Now.ToString("HH:mm:ss.fff"));
            try
            {
                Parallel.ForEach(directories, dir =>
                {
                    if (!Directory.Exists(dir))
                    {
                        DeleteFilesByDirectory(dir);
                    }
                });
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Exceptioneer.Log(e);
                    return true;
                });
            }
            directories = null;

            Console.WriteLine("{0}: Deleting unwanted extensions from database...", DateTime.Now.ToString("HH:mm:ss.fff"));
            int iDeleted = DeleteFilesByExtensions(ZapperProcessor.UnwantedExtensions);
            Console.WriteLine("{0}: {1} files deleted with unwanted extensions", DateTime.Now.ToString("HH:mm:ss.fff"), iDeleted);
            iDeleted = DeleteFilesByExtensions(ZapperProcessor.SkippedExtensions);
            Console.WriteLine("{0}: {1} files deleted with skipped extensions", DateTime.Now.ToString("HH:mm:ss.fff"), iDeleted);

            Console.WriteLine("{0}: Deleting non-existent files from database...", DateTime.Now.ToString("HH:mm:ss.fff"));
            var files = ZapperProcessor.GetFilesAll();
            try
            {
                Parallel.ForEach(files, file =>
                {
                    if (!File.Exists(file.FullPath))
                    {
                        file.DeleteFromDB(ZapperProcessor.Id, false);
                    }
                });
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Exceptioneer.Log(e);
                    return true;
                });
            }
        }

        /// <summary>
        /// Retrieve all directories that are already stored in database for at least one file
        /// </summary>
        private IEnumerable<string> GetFileDirectories()
        {
            using (var con = new SqlConnection(ZapperProcessor.Connection))
            {
                con.Open();
                string sSql = "select distinct Directory from zfile";
                return con.Query<string>(sSql);
            }
        }

        /// <summary>
        /// Delete files from the database by given directory path
        /// </summary>
        private void DeleteFilesByDirectory(string sDirPath)
        {
            using (SqlConnection con = new SqlConnection(ZapperProcessor.Connection))
            {
                using (SqlCommand cmd = new SqlCommand("delete zfile where Directory = @Directory", con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Directory", sDirPath);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Delete files from the database by given file extensions
        /// </summary>
        private int DeleteFilesByExtensions(string[] extensions)
        {
            if (extensions == null || extensions.Length == 0)
            {
                return 0;
            }

            StringBuilder sb = new StringBuilder("delete zfile where Extension in (");
            for (int i = 0; i < extensions.Length; i++)
            {
                sb.AppendFormat("{0}'{1}'",
                    i == 0 ? "" : ", ",
                    extensions[i].Replace("'", "''"));
            }
            sb.Append(")");
            string sSql = sb.ToString();
            using (SqlConnection con = new SqlConnection(ZapperProcessor.Connection))
            {
                using (SqlCommand cmd = new SqlCommand(sSql, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
