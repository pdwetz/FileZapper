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
using System.Collections.Generic;
using System.IO;
using FileZapper.Core;
using FileZapper.Core.Data;
using FileZapper.Core.Engine;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class PhaseParseFilesystemTester
    {
        [Test]
        public void process_file_system_parsing()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseParseFilesystemTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);
            rootFolder.Priority = 1;
            string keeperFilePath = Path.Combine(rootFolder.FullPath, "keeper.txt");
            ZapperFileTestHelper.CreateTextFile(keeperFilePath, 5);
            string unwantedFilePath = Path.Combine(rootFolder.FullPath, "unwanted.foo");
            ZapperFileTestHelper.CreateTextFile(unwantedFilePath, 5);
            string smallFilePath = Path.Combine(rootFolder.FullPath, "small.txt");
            ZapperFileTestHelper.CreateTextFile(smallFilePath, 1);
            string largeFilePath = Path.Combine(rootFolder.FullPath, "large.txt");
            ZapperFileTestHelper.CreateTextFile(largeFilePath, 10);

            var settings = new FileZapperSettings
            {
                IgnoreFilesBelowBytes = 1000,
                IgnoreFilesOverBytes = 3000,
                SkippedExtensions = new string[] { },
                UnwantedExtensions = new string[] { ".foo" },
                UnwantedFolders = new string[] { },
                RootFolders = new List<ZapperFolder> { rootFolder }
            };

            var allphases = new List<IZapperPhase>();
            var phase = new PhaseParseFilesystem { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            phase.Process();
            Assert.IsTrue(File.Exists(keeperFilePath));
            Assert.IsFalse(File.Exists(unwantedFilePath));
            Assert.AreEqual(1, processor.ZapperFiles.Count);
            Assert.AreEqual(1, processor.ZapperFilesDeleted.Count);
        }
    }
}