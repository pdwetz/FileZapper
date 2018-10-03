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
using FileZapper.Core;
using FileZapper.Core.Data;
using FileZapper.Core.Engine;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class PhaseCleanupTester
    {
        [Test]
        public void process_cleanup()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCleanupTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            string sUnwantedFolderName = "unwanted";
            var wantedFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "wanted");
            var unwantedFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, sUnwantedFolderName);

            FileZapperSettings settings = new FileZapperSettings();
            settings.UnwantedFolders = new string[] { sUnwantedFolderName };
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseCleanup { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            phase.Process();
            Assert.IsTrue(Directory.Exists(wantedFolder.FullPath));
            Assert.IsTrue(!Directory.Exists(unwantedFolder.FullPath));
        }
    }
}