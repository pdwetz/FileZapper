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
    public class PhaseParseFilesystem : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; } = "Parse file system";
        public bool IsInitialPhase { get; set; }

        private readonly ILogger _log;

        public PhaseParseFilesystem()
        {
            _log = Log.ForContext<PhaseParseFilesystem>();
        }

        public void Process()
        {
            _log.Information(Name);
            foreach (var root in ZapperProcessor.Settings.RootFolders)
            {
                _log.Information("Parsing folder {FullPath}", root.FullPath);
                var filepaths = Directory.EnumerateFiles(root.FullPath, "*.*", System.IO.SearchOption.AllDirectories);
                try
                {
                    Parallel.ForEach(filepaths, filePath =>
                    {
                        if (filePath.Length >= 260)
                        {
                            _log.Warning("Path too long - {FilePath}", filePath);
                        }
                        else
                        {
                            var zfile = new ZapperFile(filePath);
                            if (!zfile.IsSystem)
                            {
                                if (ZapperProcessor.Settings.UnwantedExtensions.Contains(zfile.Extension))
                                {
                                    FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                    ZapperFileDeleted zfiledeleted = new ZapperFileDeleted(zfile, ZapperProcessor.ZapperSession.Id);
                                    if (!ZapperProcessor.ZapperFilesDeleted.TryAdd(zfiledeleted.FullPath, zfiledeleted))
                                    {
                                        throw new FileZapperAddToDictionaryFailureException("ZapperFilesDeleted", zfiledeleted.FullPath);
                                    }
                                }
                                else if (!ZapperProcessor.Settings.SkippedExtensions.Contains(zfile.Extension)
                                    && zfile.Size > ZapperProcessor.Settings.IgnoreFilesBelowBytes
                                    && (ZapperProcessor.Settings.IgnoreFilesOverBytes <= 0 || zfile.Size < ZapperProcessor.Settings.IgnoreFilesOverBytes))
                                {
                                    if (!ZapperProcessor.ZapperFiles.TryAdd(zfile.FullPath, zfile))
                                    {
                                        throw new FileZapperAddToDictionaryFailureException("ZapperFiles", zfile.FullPath);
                                    }
                                }
                            }
                        }
                    });
                }
                catch (AggregateException ae)
                {
                    ae.Handle(e =>
                    {
                        _log.Error(e, "Error while parsing file system");
                        return true;
                    });
                }
            }
        }
    }
}