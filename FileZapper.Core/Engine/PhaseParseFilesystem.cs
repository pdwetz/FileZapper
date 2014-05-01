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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileZapper.Core.Data;
using FileZapper.Core.Utilities;
using log4net;
using Microsoft.VisualBasic.FileIO;

namespace FileZapper.Core.Engine
{
    public class PhaseParseFilesystem : IZapperPhase
    {
        private readonly ILog _log = LogManager.GetLogger("PhaseParseFilesystem");
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseParseFilesystem()
        {
            Name = "Save file system info to database";
        }

        public void Process()
        {
            foreach (var root in ZapperProcessor.Settings.RootFolders)
            {
                Console.WriteLine("{0}: Parsing folder {1}", DateTime.Now.ToString("HH:mm:ss.fff"), root.FullPath);
                var filepaths = Directory.EnumerateFiles(root.FullPath, "*.*", System.IO.SearchOption.AllDirectories);
                try
                {
                    Parallel.ForEach(filepaths, filepath =>
                    {
                        if (filepath.Length >= 260)
                        {
                            Console.WriteLine("{0}: Path too long - {1}", DateTime.Now.ToString("HH:mm:ss.fff"), filepath);
                        }
                        else
                        {
                            var zfile = new ZapperFile(filepath);
                            if (!zfile.IsSystem)
                            {
                                if (ZapperProcessor.Settings.UnwantedExtensions.Contains(zfile.Extension))
                                {
                                    FileSystem.DeleteFile(filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
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
                        Exceptioneer.Log(_log, e);
                        return true;
                    });
                }
            }
        }
    }
}