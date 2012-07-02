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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace FileZapper
{
    /// <summary>
    /// Parse all of our root folders for new/updated file info
    /// </summary>
    class PhaseParseFilesystem : IZapperPhase
    {
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
            var files = ZapperProcessor.GetFilesAll();
            var masterfiles = files.ToDictionary<ZapperFile, string>(x => x.FullPath);
            // Each root is done in sequence (Path in DB is clustered PK); internally within root, parallel.
            foreach (var root in ZapperProcessor.EnabledRootFolders)
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
                            ZapperFile file = new ZapperFile(root, filepath);
                            if (!file.IsSystem)
                            {
                                if (ZapperProcessor.UnwantedExtensions.Contains(file.Extension))
                                {
                                    FileSystem.DeleteFile(filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                }
                                else if (!ZapperProcessor.SkippedExtensions.Contains(file.Extension))
                                {
                                    if (!masterfiles.ContainsKey(file.FullPath)
                                        ||
                                        (masterfiles.ContainsKey(file.FullPath)
                                        && !file.EqualsIgnoreHash(masterfiles[file.FullPath])))
                                    {
                                        file.SaveToDB();
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
                        Exceptioneer.Log(e);
                        return true;
                    });
                }
            }
            masterfiles = null;
        }
    }
}
