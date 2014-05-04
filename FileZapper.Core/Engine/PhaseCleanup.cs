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
using FileZapper.Core.Utilities;
using log4net;
using Microsoft.VisualBasic.FileIO;

namespace FileZapper.Core.Engine
{
    public class PhaseCleanup : IZapperPhase
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PhaseCleanup));
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
            _log.Info(Name);

            if (ZapperProcessor == null) { throw new Exception("ZapperProcessor required"); }
            if (ZapperProcessor.Settings == null) { throw new Exception("ZapperProcessor.Settings required"); }
            if (ZapperProcessor.Settings.UnwantedFolders == null) { throw new Exception("ZapperProcessor.Settings.UnwantedFolders required"); }

            if (!ZapperProcessor.Settings.UnwantedFolders.Any())
            {
                System.Diagnostics.Trace.WriteLine("No unwanted folder names, skipping phase: " + Name);
                return;
            }
            Console.WriteLine("{0}: Deleting unwanted directories from file system...", DateTime.Now.ToString("HH:mm:ss.fff"));
            foreach (var root in ZapperProcessor.Settings.RootFolders)
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
                            if (ZapperProcessor.Settings.UnwantedFolders.Contains(d.Name))
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
                        Exceptioneer.Log(_log, e);
                        return true;
                    });
                }
            }
        }
    }
}