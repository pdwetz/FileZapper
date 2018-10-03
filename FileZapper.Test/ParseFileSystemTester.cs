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
using System.IO;
using FileZapper.Core.Data;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class ParseFileSystemTester
    {
        private const string SmallFileName = "small.txt";
        private const string MediumFileName = "medium.txt";
        private const string LargeFileName = "large.txt";

        private ZapperFolder _rootFolder;

        [SetUp]
        public void SetUp()
        {
            _rootFolder = ZapperFileTestHelper.GetTestFileRoot();
            System.Diagnostics.Trace.WriteLine(_rootFolder.FullPath);
        }

        [Test]
        public void load_small_file()
        {
            string sFilePath = Path.Combine(_rootFolder.FullPath, SmallFileName);
            ZapperFileTestHelper.CreateTextFile(sFilePath, 5);
            var zfile = new ZapperFile(sFilePath);
            Assert.IsNotNull(zfile);
            Assert.AreEqual(zfile.Name, SmallFileName);
            Assert.AreNotEqual(zfile.Size, 0);
            Assert.IsFalse(zfile.IsSystem);
            Assert.AreEqual(zfile.Directory, _rootFolder.FullPath);
            Assert.AreEqual(zfile.Extension, ".txt");
        }

        [Test]
        public void load_medium_file()
        {
            string sFilePath = Path.Combine(_rootFolder.FullPath, MediumFileName);
            ZapperFileTestHelper.CreateTextFile(sFilePath, 500);
            var zfile = new ZapperFile(sFilePath);
            Assert.IsNotNull(zfile);
            Assert.AreEqual(zfile.Name, MediumFileName);
            Assert.AreNotEqual(zfile.Size, 0);
            Assert.IsFalse(zfile.IsSystem);
            Assert.AreEqual(zfile.Directory, _rootFolder.FullPath);
            Assert.AreEqual(zfile.Extension, ".txt");
        }

        [Test]
        public void load_large_file()
        {
            string sFilePath = Path.Combine(_rootFolder.FullPath, LargeFileName);
            ZapperFileTestHelper.CreateTextFile(sFilePath, 50000);
            var zfile = new ZapperFile(sFilePath);
            Assert.IsNotNull(zfile);
            Assert.AreEqual(zfile.Name, LargeFileName);
            Assert.AreNotEqual(zfile.Size, 0);
            Assert.IsFalse(zfile.IsSystem);
            Assert.AreEqual(zfile.Directory, _rootFolder.FullPath);
            Assert.AreEqual(zfile.Extension, ".txt");
        }
    }
}