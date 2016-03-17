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
    public class PhaseCalculateHashesTester
    {
        [Test]
        public void matching_files()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateHashesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            List<ZapperFile> files = new List<ZapperFile>();
            string sFilePath = Path.Combine(rootFolder.FullPath, "alpha.txt");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 500, Extension = ".txt", SampleHash = "1" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            sFilePath = Path.Combine(rootFolder.FullPath, "bravo.txt");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 500, Extension = ".txt", SampleHash = "1" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateHashes { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }
            
            phase.Process();

            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].ContentHash, Is.Not.Null.And.Not.Empty);
            Assert.That(files[1].ContentHash, Is.Not.Null.And.Not.Empty);
            Assert.AreEqual(files[0].ContentHash, files[1].ContentHash);
        }

        [Test]
        public void different_extensions_same_content()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateHashesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            List<ZapperFile> files = new List<ZapperFile>();

            string sFilePath = Path.Combine(rootFolder.FullPath, "charlie.txt");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 999, Extension = ".txt", SampleHash = "2" });

            sFilePath = Path.Combine(rootFolder.FullPath, "delta.foo");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 999, Extension = ".foo", SampleHash = "2" });

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateHashes { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            phase.Process();

            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].ContentHash, Is.Null.Or.Empty);
            Assert.That(files[1].ContentHash, Is.Null.Or.Empty);
        }

        [Test]
        public void empty_sample_hashes()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateHashesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            List<ZapperFile> files = new List<ZapperFile>();
            string sFilePath = Path.Combine(rootFolder.FullPath, "alpha.txt");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 500, Extension = ".txt" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            sFilePath = Path.Combine(rootFolder.FullPath, "bravo.txt");
            files.Add(new ZapperFile { FullPath = sFilePath, Size = 500, Extension = ".txt" });
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);

            FileZapperSettings settings = new FileZapperSettings();
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(rootFolder);
            settings.RootFolders = folders;

            List<IZapperPhase> allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateHashes { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            phase.Process();

            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].ContentHash, Is.Null.Or.Empty);
            Assert.That(files[1].ContentHash, Is.Null.Or.Empty);
        }
    }
}