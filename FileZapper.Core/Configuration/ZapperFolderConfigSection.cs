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
using System.Configuration;

namespace FileZapper.Core.Configuration
{
    public class ZapperFolderConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("IgnoreFilesBelowBytes", DefaultValue = "0")]
        public long IgnoreFilesBelowBytes { get { return (long)this["IgnoreFilesBelowBytes"]; } }

        [ConfigurationProperty("IgnoreFilesOverBytes", DefaultValue = "0")]
        public long IgnoreFilesOverBytes { get { return (long)this["IgnoreFilesOverBytes"]; } }

        [ConfigurationProperty("SkippedExtensions", DefaultValue = "")]
        public string SkippedExtensions { get { return this["SkippedExtensions"] as string; } }

        [ConfigurationProperty("UnwantedExtensions", DefaultValue = "")]
        public string UnwantedExtensions { get { return this["UnwantedExtensions"] as string; } }

        [ConfigurationProperty("UnwantedFolders", DefaultValue = "")]
        public string UnwantedFolders { get { return this["UnwantedFolders"] as string; } }

        [ConfigurationProperty("ZapperFolders")]
        public ZapperFolderConfigCollection Folders
        {
            get { return this["ZapperFolders"] as ZapperFolderConfigCollection; }
        }
    }
}