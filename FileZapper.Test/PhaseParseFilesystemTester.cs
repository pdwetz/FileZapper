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
    public class PhaseParseFilesystemTester
    {
        [Test]
        public void process()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseParseFilesystemTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);
            rootFolder.Priority = 1;
            string sKeeperFilePath = Path.Combine(rootFolder.FullPath, "keeper.txt");
            ZapperFileTestHelper.CreateTextFile(sKeeperFilePath, 5);
            string sUnwantedFilePath = Path.Combine(rootFolder.FullPath, "unwanted.foo");
            ZapperFileTestHelper.CreateTextFile(sUnwantedFilePath, 5);
            string sSmallFilePath = Path.Combine(rootFolder.FullPath, "small.txt");
            ZapperFileTestHelper.CreateTextFile(sSmallFilePath, 1);
            string sLargeFilePath = Path.Combine(rootFolder.FullPath, "large.txt");
            ZapperFileTestHelper.CreateTextFile(sLargeFilePath, 10);

            FileZapperSettings settings = new FileZapperSettings();
            settings.UnwantedExtensions = new string[] { ".foo" };
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;
            settings.IgnoreFilesBelowBytes = 1000;
            settings.IgnoreFilesOverBytes = 3000;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseParseFilesystem { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            phase.Process();
            Assert.IsTrue(File.Exists(sKeeperFilePath));
            Assert.IsFalse(File.Exists(sUnwantedFilePath));
            Assert.AreEqual(1, processor.ZapperFiles.Count);
            Assert.AreEqual(1, processor.ZapperFilesDeleted.Count);
        }
    }
}