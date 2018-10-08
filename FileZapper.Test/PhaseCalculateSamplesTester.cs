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
    public class PhaseCalculateSamplesTester
    {
        [Test]
        public void process_samples()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateSamplesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            var files = new List<ZapperFile>();

            string filePath = Path.Combine(rootFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var alpha = new ZapperFile(filePath);
            files.Add(alpha);

            filePath = Path.Combine(rootFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var bravo = new ZapperFile(filePath);
            files.Add(bravo);

            filePath = Path.Combine(rootFolder.FullPath, "charlie.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50, ZapperFileTestHelper.AltFillerText);
            var charlie = new ZapperFile(filePath);
            files.Add(charlie);

            var settings = new FileZapperSettings();
            var folders = new List<ZapperFolder>
            {
                rootFolder
            };
            settings.RootFolders = folders;

            var allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateSamples { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            phase.Process();

            Assert.AreEqual(3, processor.ZapperFiles.Count);
            Assert.That(files[0].SampleHash, Is.Not.Null.And.Not.Empty);
            Assert.AreEqual(files[0].SampleHash, files[1].SampleHash);
            Assert.That(files[2].SampleHash, Is.Null.Or.Empty);
        }

        [Test]
        public void process_folders_ignorehierarchy()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateSamplesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            var alphaFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Alpha");
            var bravoFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Bravo");

            var files = new List<ZapperFile>();

            string filePath = Path.Combine(alphaFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var alpha = new ZapperFile(filePath);
            files.Add(alpha);

            filePath = Path.Combine(bravoFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var bravo = new ZapperFile(filePath);
            files.Add(bravo);

            var settings = new FileZapperSettings();
            var folders = new List<ZapperFolder>
            {
                rootFolder
            };
            settings.RootFolders = folders;

            var allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateSamples { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            settings.DupeCheckIgnoresHierarchy = true;
            phase.Process();
            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].SampleHash, Is.Not.Null.And.Not.Empty);
            Assert.AreEqual(files[0].SampleHash, files[1].SampleHash);
        }

        [Test]
        public void process_folders_hierarchyonly_singleroot()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateSamplesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            var alphaFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Alpha");
            var bravoFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Bravo");

            var files = new List<ZapperFile>();

            string filePath = Path.Combine(alphaFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var alpha = new ZapperFile(filePath);
            files.Add(alpha);

            filePath = Path.Combine(bravoFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var bravo = new ZapperFile(filePath);
            files.Add(bravo);

            var settings = new FileZapperSettings();
            var folders = new List<ZapperFolder>
            {
                rootFolder
            };
            settings.RootFolders = folders;

            var allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateSamples { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            settings.DupeCheckIgnoresHierarchy = false;
            phase.Process();
            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].SampleHash, Is.Null.Or.Empty);
            Assert.That(files[1].SampleHash, Is.Null.Or.Empty);
        }

        [Test]
        public void process_folders_hierarchyonly_multiroot()
        {
            var rootFolder = ZapperFileTestHelper.GetTestFileSubfolder("PhaseCalculateSamplesTester");
            System.Diagnostics.Trace.WriteLine(rootFolder.FullPath);

            var alphaFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Alpha");
            alphaFolder.Priority = 300000;
            var bravoFolder = ZapperFileTestHelper.GetTestFileSubfolder(rootFolder.FullPath, "Bravo");
            bravoFolder.Priority = 100000;

            var files = new List<ZapperFile>();

            string filePath = Path.Combine(alphaFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var alpha = new ZapperFile(filePath);
            files.Add(alpha);

            filePath = Path.Combine(bravoFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(filePath, 50);
            var bravo = new ZapperFile(filePath);
            files.Add(bravo);

            var settings = new FileZapperSettings();
            var folders = new List<ZapperFolder>
            {
                alphaFolder,
                bravoFolder
            };
            settings.RootFolders = folders;

            var allphases = new List<IZapperPhase>();
            var phase = new PhaseCalculateSamples { PhaseOrder = 1, IsInitialPhase = true };
            allphases.Add(phase);

            var processor = new ZapperProcessor(settings, allphases);
            foreach (var zfile in files)
            {
                Assert.IsTrue(processor.ZapperFiles.TryAdd(zfile.FullPath, zfile));
            }

            settings.DupeCheckIgnoresHierarchy = false;
            phase.Process();
            Assert.AreEqual(2, processor.ZapperFiles.Count);
            Assert.That(files[0].SampleHash, Is.Null.Or.Empty);
            Assert.That(files[1].SampleHash, Is.Null.Or.Empty);
        }
    }
}
