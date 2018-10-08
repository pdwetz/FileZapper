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
using FileZapper.Core;
using FileZapper.Core.Data;
using FileZapper.Core.Engine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileZapper.Test
{
    [TestFixture]
    public class ZapperProcessorTester
    {
        private ZapperProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            var settings = new FileZapperSettings
            {
                IgnoreFilesBelowBytes = 0,
                IgnoreFilesOverBytes = long.MaxValue,
                SkippedExtensions = new string[] { },
                UnwantedExtensions = new string[] { },
                UnwantedFolders = new string[] { },
                RootFolders = new List<ZapperFolder> { new ZapperFolder { FullPath = "test path 1", Priority = 150 } }
            };
            var allphases = new List<IZapperPhase>
            {
                new TestPhase { PhaseOrder = 1, Name = "Alpha", IsInitialPhase = true },
                new TestPhase { PhaseOrder = 2, Name = "Bravo" },
                new TestPhase { PhaseOrder = 3, Name = "Charlie" }
            };

            _processor = new ZapperProcessor(settings, allphases);
        }

        [Test]
        public void process_all()
        {
            _processor.Process();
            Assert.IsTrue(_processor.ZapperSession.Id != Guid.Empty);
            Assert.AreEqual(3, _processor.ZapperSession.PhasesProcessed);
            Assert.IsTrue(_processor.ZapperSession.EndDate.HasValue && _processor.ZapperSession.EndDate.Value > DateTime.MinValue);
        }

        [Test]
        public void process_skip_one()
        {
            _processor.ZapperSession.CurrentPhase = 2;
            _processor.Process();

            Assert.IsTrue(_processor.ZapperSession.Id != Guid.Empty);
            Assert.AreEqual(2, _processor.ZapperSession.PhasesProcessed);
            Assert.IsTrue(_processor.ZapperSession.EndDate.HasValue && _processor.ZapperSession.EndDate.Value > DateTime.MinValue);
        }

        [Test]
        public void log_results()
        {
            string logPathResult = _processor.LogResults(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(TestContext.CurrentContext.TestDirectory, logPathResult);
            string filePath = Path.Combine(logPathResult, "zappersessions.csv");
            Assert.IsTrue(File.Exists(filePath));
        }
    }

    public class TestPhase : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public void Process() { }
    }
}