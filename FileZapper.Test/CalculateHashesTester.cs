/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2013 Peter Wetzel

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
using FileZapper.Core.Engine;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class CalculateHashesTester
    {
        private ZapperFolder _rootFolder;

        [SetUp]
        public void SetUp()
        {
            _rootFolder = ZapperFileTestHelper.GetTestFileRoot();

            string sFilePath = Path.Combine(_rootFolder.FullPath, "alpha.txt");
            ZapperFileTestHelper.CreateTextFile(sFilePath, 10);

            sFilePath = Path.Combine(_rootFolder.FullPath, "bravo.txt");
            ZapperFileTestHelper.CreateTextFile(sFilePath, 10);
        }

        [Test]
        public async void calculate_hash_single_file()
        {
            string sFilePath = Path.Combine(_rootFolder.FullPath, "alpha.txt");
            string sHash = await PhaseCalculateHashes.CalculateMD5Hash(sFilePath);
            Assert.IsNotNullOrEmpty(sHash);
            System.Diagnostics.Trace.WriteLine("Hash for " + sFilePath + ": " + sHash);
        }

        [Test]
        public async void calculate_hash_duplicate_files()
        {
            string sFilePath = Path.Combine(_rootFolder.FullPath, "alpha.txt");
            string sHashAlpha = await PhaseCalculateHashes.CalculateMD5Hash(sFilePath);
            Assert.IsNotNullOrEmpty(sHashAlpha);
            System.Diagnostics.Trace.WriteLine("Hash for " + sFilePath + ": " + sHashAlpha);

            sFilePath = Path.Combine(_rootFolder.FullPath, "bravo.txt");
            string sHashBravo = await PhaseCalculateHashes.CalculateMD5Hash(sFilePath);
            Assert.IsNotNullOrEmpty(sHashBravo);

            Assert.AreEqual(sHashAlpha, sHashBravo);
        }
    }
}
