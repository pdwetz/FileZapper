/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2017 Peter Wetzel

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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileZapper.Core.Data;
using Microsoft.VisualBasic.FileIO;
using Serilog;

namespace FileZapper.Core.Engine
{
    /// <summary>
    /// Banish duplicate files to recycling bin
    /// </summary>
    public class PhaseRemoveDuplicates : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; } = "Remove duplicate files";
        public bool IsInitialPhase { get; set; }

        private readonly ILogger _log;

        public PhaseRemoveDuplicates()
        {
            _log = Log.ForContext<PhaseRemoveDuplicates>();
        }

        public void Process()
        {
            _log.Information(Name);
            var files = ZapperProcessor.ZapperFiles.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.ContentHash) && !x.ContentHash.Equals("invalid", StringComparison.InvariantCultureIgnoreCase));
            _log.Information("Calculating file scores");
            try
            {
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, zfile =>
                {
                    CalculateScore(zfile);
                });
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    _log.Error(e, "Error while calculating scores");
                    return true;
                });
            }

            var dupes = (from z in files
                         group z by z.ContentHash into g
                         select new { ContentHash = g.Key, Count = g.Count(), Files = g })
                         .Where(x => x.Count > 1);
            if (!dupes.Any())
            {
                _log.Information("No duplicates found");
                return;
            }

            _log.Information("Deleting duplicates");
            try
            {
                Parallel.ForEach(dupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupe =>
                {
                    var deadmenwalking = (from z in dupe.Files
                                          orderby z.Score descending
                                          select z).Skip(1);

                    // Not parallel; for large workloads this spammed the thread pool, even when limited to logical processor count
                    foreach (var dead in deadmenwalking)
                    {
                        if (File.Exists(dead.FullPath))
                        {
                            FileSystem.DeleteFile(dead.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                        ZapperFileDeleted zfiledeleted = new ZapperFileDeleted(dead, ZapperProcessor.ZapperSession.Id);
                        if (!ZapperProcessor.ZapperFilesDeleted.TryAdd(zfiledeleted.FullPath, zfiledeleted))
                        {
                            throw new FileZapperAddToDictionaryFailureException("ZapperFilesDeleted", zfiledeleted.FullPath);
                        }
                        ZapperFile killed;
                        if (!ZapperProcessor.ZapperFiles.TryRemove(dead.FullPath, out killed))
                        {
                            throw new FileZapperRemoveFromDictionaryFailureException("ZapperFiles", dead.FullPath);
                        }
                    }
                });
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    _log.Error(e, "Error during file deletion");
                    return true;
                });
            }
        }

        /// <summary>
        /// Rudimentary scoring mechanism for files; used to determine which file "wins" for duplicates. Scores do not have to be unique.
        /// </summary>
        public void CalculateScore(ZapperFile zfile)
        {
            var root = ZapperProcessor.Settings.RootFolders.FirstOrDefault(x => zfile.Directory.StartsWith(x.FullPath, StringComparison.OrdinalIgnoreCase));
            // Assumes "core" folders have 6 figure priority
            int iRootScore = root == null ? 0 : root.Priority;
            // Assumes named folders should take priority over misc folders
            int iNotMiscScore = zfile.Directory.Contains("misc") || zfile.Directory.Contains("unfiltered") ? 0 : 10000;
            // Assumes deeply nested is better than not
            int iNestScore = (zfile.Directory.Count(x => x == '\\') + 1) * 1000;
            // Assumes older is better
            int iTimeScore = Convert.ToInt32((DateTime.Now - (zfile.FileModified ?? DateTime.Now)).TotalDays / 365);
            zfile.Score = iRootScore + iNotMiscScore + iNestScore + iTimeScore;
        }
    }
}