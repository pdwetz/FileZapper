/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2018 Peter Wetzel

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
using System.IO;
using System.Linq;
using FileZapper.Core;
using FileZapper.Core.Data;
using FileZapper.Core.Engine;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class PhaseRemoveDuplicatesTester
    {
        [Test]
        public void calculate_score()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseRemoveDuplicatesTester");
            rootFolder.Priority = 100000;
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseRemoveDuplicates { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            var zfile = new ZapperFile { Directory = rootFolder.FullPath, FullPath = Path.Combine(rootFolder.FullPath, "test.txt"), FileModified = DateTime.Now };
            phase.CalculateScore(zfile);
            int iScore = 110000 + ((zfile.Directory.Count(x => x == '\\') + 1) * 1000);
            Assert.AreEqual(iScore, zfile.Score);
        }

        [Test]
        public void process_duplicates()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseRemoveDuplicatesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            var importantFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "important");
            importantFolder.Priority = 900000;
            var loserFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "loser");
            loserFolder.Priority = 100000;

            List<ZapperFile> files = new List<ZapperFile>();
            string sFilePath = Path.Combine(importantFolder.FullPath, "alpha.txt");
            files.Add(new ZapperFile { Directory = importantFolder.FullPath, FullPath = sFilePath, FileModified = DateTime.Now, ContentHash = "something" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            sFilePath = Path.Combine(loserFolder.FullPath, "bravo.txt");
            files.Add(new ZapperFile { Directory = loserFolder.FullPath, FullPath = sFilePath, FileModified = DateTime.Now, ContentHash = "something" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(importantFolder);
            folders.Add(loserFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseRemoveDuplicates { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            phase.Process();

            Assert.AreEqual(1, processor.ZapperFiles.Count);
            Assert.AreEqual(1, processor.ZapperFilesDeleted.Count);
        }
    }
}