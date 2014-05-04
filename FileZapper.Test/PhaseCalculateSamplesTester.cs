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
using System.Collections.Generic;
using System.IO;
using FileZapper.Core.Configuration;
using FileZapper.Core.Data;
using FileZapper.Core.Engine;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class PhaseCalculateSamplesTester
    {
        [Test]
        public void process()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateSamplesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            List<ZapperFile> files = new List<ZapperFile>();

            string sFilePath = Path.Combine(rootFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(sFilePath, 50);
            var alpha = new ZapperFile(sFilePath);
            files.Add(alpha);

            sFilePath = Path.Combine(rootFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(sFilePath, 50);
            var bravo = new ZapperFile(sFilePath);
            files.Add(bravo);

            sFilePath = Path.Combine(rootFolder.FullPath, "charlie.txt");
            ZapperFileTestHelper.CreateTextFile(sFilePath, 50, ZapperFileTestHelper.AltFillerText);
            var charlie = new ZapperFile(sFilePath);
            files.Add(charlie);

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateSamples { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            phase.Process();

            Assert.AreEqual(3, processor.ZapperFiles.Count);
            Assert.IsNotNullOrEmpty(files[0].SampleHash);
            Assert.AreEqual(files[0].SampleHash, files[1].SampleHash);
            Assert.IsNullOrEmpty(files[2].SampleHash);
        }
    }
}
