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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualBasic.FileIO;

namespace FileZapper
{
    /// <summary>
    /// Banish duplicate files to recycling bin
    /// </summary>
    class PhaseRemoveDuplicates : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseRemoveDuplicates()
        {
            Name = "Remove duplicate files";
        }

        public void Process()
        {
            var files = GetDuplicateHashes();
            Console.WriteLine("{0}: Calc file scores", DateTime.Now.ToString("HH:mm:ss.fff"));
            var AllRootFolders = GetFoldersAll();
            try
            {
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, hashfile =>
                {
                    hashfile.Root = AllRootFolders.FirstOrDefault(x => hashfile.Directory.StartsWith(x.FullPath, StringComparison.OrdinalIgnoreCase));
                    // Assumes "core" folders have 6 figure priority
                    int iRootScore = hashfile.Root == null ? 0 : hashfile.Root.Priority;
                    // Assumes named folders should take priority over misc folders
                    int iNotMiscScore = hashfile.Directory.Contains("misc") || hashfile.Directory.Contains("unfiltered") ? 0 : 10000;
                    // Assumes deeply nested is better than not
                    int iNestScore = (hashfile.Directory.Count(x => x == '/') + 1) * 1000;
                    // Assumes older is better
                    int iTimeScore = Convert.ToInt32((DateTime.Now - (hashfile.FileModified ?? DateTime.Now)).TotalDays / 365);
                    hashfile.Score = iRootScore + iNotMiscScore + iNestScore + iTimeScore;
                    //Console.WriteLine("{0}: {1}", hashfile.Score, hashfile.FullPath);
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

            Console.WriteLine("{0}: Delete losers", DateTime.Now.ToString("HH:mm:ss.fff"));
            var dupes = (from z in files
                         group z by z.ContentHash into g
                         select new { ContentHash = g.Key, Count = g.Count(), Files = g });
            try
            {
                Parallel.ForEach(dupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupe =>
                {
                    var deadmenwalking = (from z in dupe.Files
                                          orderby z.Score descending
                                          select z).Skip(1);
                    // Not parallel; for large workloads this spammed the threadpool, even when limited to logical processor count
                    foreach (var dead in deadmenwalking)
                    {
                        if (File.Exists(dead.FullPath))
                        {
                            FileSystem.DeleteFile(dead.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                        dead.DeleteFromDB(ZapperProcessor.Id);
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
        /// Retrieve all explicitly tracked folders; they may or may not have been processed before.
        /// </summary>
        private IEnumerable<ZapperFolder> GetFoldersAll()
        {
            using (var con = new SqlConnection(ZapperProcessor.Connection))
            {
                con.Open();
                string sSql = @"select * from zfolder";
                return con.Query<ZapperFolder>(sSql);
            }
        }

        /// <summary>
        /// Retrieve all files from database that have duplicate hashes
        /// </summary>
        private IEnumerable<ZapperFile> GetDuplicateHashes()
        {
            using (var con = new SqlConnection(ZapperProcessor.Connection))
            {
                con.Open();
                string sSql = @"
                    with Dupes (ContentHash)
                    as
                    (
	                    select ContentHash
	                    from zfile
                        where ContentHash is not null and ContentHash != 'INVALID'
	                    group by ContentHash
	                    having count(*) > 1
                    )
                    select f.*
                    from zfile f, Dupes d
                    where f.ContentHash = d.ContentHash
                    ";

                return con.Query<ZapperFile>(sSql);
            }
        }
    }
}
