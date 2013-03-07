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
using System.Collections.Generic;
using System.Linq;
using FileZapper.Core.Configuration;
using FileZapper.Core.Data;
using NUnit.Framework;

namespace FileZapper.Test
{
    [TestFixture]
    public class SettingsTester
    {
        [Test]
        public void settings_loaded_programmatically()
        {
            FileZapperSettings settings = new FileZapperSettings();
            settings.IgnoreFilesBelowBytes = 50;
            settings.SkippedExtensions = new string[] { ".foo", ".bar" };
            List<ZapperFolder> folders = new List<ZapperFolder>();
            folders.Add(new ZapperFolder { FullPath = "test path 1", Priority = 150 });
            folders.Add(new ZapperFolder { FullPath = "test path 2", Priority = 50 });
            settings.RootFolders = folders;
            
            Assert.AreEqual(50, settings.IgnoreFilesBelowBytes);
            Assert.AreEqual(0, settings.IgnoreFilesOverBytes);
            Assert.AreEqual(2, settings.SkippedExtensions.Count());
            Assert.IsEmpty(settings.UnwantedExtensions);
            Assert.AreEqual(2, settings.RootFolders.Count);
        }

        [Test]
        public void settings_loaded_from_file()
        {
            FileZapperSettings settings = new FileZapperSettings();
            settings.Load();
            Assert.AreEqual(100, settings.IgnoreFilesBelowBytes);
            Assert.AreEqual(500000, settings.IgnoreFilesOverBytes);
            Assert.AreEqual(1, settings.UnwantedFolders.Count());
            Assert.AreEqual(2, settings.RootFolders.Count);
        }
    }
}