/*
    FileZapper - Finds and removed duplicate files
    Copyright (C) 2017 Peter Wetzel

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
using FileZapper.Core.Data;

namespace FileZapper.Core
{
    public class FileZapperSettings
    {
        public string Hasher { get; set; } = "Farmhash";
        public bool DupeCheckIgnoresHierarchy { get; set; }
        public long IgnoreFilesBelowBytes { get; set; } = 0;
        public long IgnoreFilesOverBytes { get; set; } = long.MaxValue;
        public string[] SkippedExtensions { get; set; } = new string[] { };
        public string[] UnwantedExtensions { get; set; } = new string[] { };
        public string[] UnwantedFolders { get; set; } = new string[] { };
        public List<ZapperFolder> RootFolders { get; set; } = new List<ZapperFolder>();
    }
}