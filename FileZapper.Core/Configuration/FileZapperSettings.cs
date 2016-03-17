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
using System;
using System.Collections.Generic;
using System.Configuration;
using FileZapper.Core.Data;

namespace FileZapper.Core.Configuration
{
    public class FileZapperSettings
    {
        public bool DupeCheckIgnoresHierarchy { get; set; }
        public long IgnoreFilesBelowBytes { get; set; }
        public long IgnoreFilesOverBytes { get; set; }
        public string[] SkippedExtensions { get; set; }
        public string[] UnwantedExtensions { get; set; }
        public string[] UnwantedFolders { get; set; }
        public List<ZapperFolder> RootFolders { get; set; }

        public FileZapperSettings()
        {
            SkippedExtensions = new string[] { };
            UnwantedExtensions = new string[] { };
            UnwantedFolders = new string[] { };
            RootFolders = new List<ZapperFolder>();
        }

        public void Load()
        {
            var _config = ConfigurationManager.GetSection("FileZapper") as ZapperFolderConfigSection;
            if (_config == null)
            {
                throw new Exception("FileZapper config section missing");
            }
            DupeCheckIgnoresHierarchy = _config.DupeCheckIgnoresHierarchy;
            IgnoreFilesBelowBytes = _config.IgnoreFilesBelowBytes;
            IgnoreFilesOverBytes = _config.IgnoreFilesOverBytes;
            SkippedExtensions = (_config.SkippedExtensions ?? "").Split(new char[] { ',' });
            UnwantedExtensions = (_config.UnwantedExtensions ?? "").Split(new char[] { ',' });
            UnwantedFolders = (_config.UnwantedFolders ?? "").Split(new char[] { ',' });
            if (_config.Folders == null)
            {
                throw new Exception("FileZapper folders config section missing");
            }
            foreach (ZapperFolderConfigElement folder in _config.Folders)
            {
                RootFolders.Add(new ZapperFolder
                {
                    FullPath = folder.FullPath,
                    Priority = folder.Priority
                });
            }
        }
    }
}