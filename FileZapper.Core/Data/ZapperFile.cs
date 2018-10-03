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
using System;
using System.IO;

namespace FileZapper.Core.Data
{
    public class ZapperFile : IEquatable<ZapperFile>
    {
        public const int DefaultSampleBytesOffset = 10240;
        public const int DefaultSampleBytesSize = 1024;

        public string FullPath { get; set; }
        public string Name { get; set; }
        public string ContentHash { get; set; }
        public string Extension { get; set; }
        public string Directory { get; set; }
        public long Size { get; set; }
        public DateTime? FileModified { get; set; }
        public long HashTime { get; set; }
        public bool IsSystem { get; set; }
        public int Score { get; set; }
        public int SampleBytesOffset { get; set; }
        public int SampleBytesSize { get; set; }
        public string SampleHash { get; set; }

        public ZapperFile()
        {
            SampleBytesOffset = DefaultSampleBytesOffset;
            SampleBytesSize = DefaultSampleBytesSize;
        }

        public ZapperFile(string filePath)
            : this()
        {
            FullPath = filePath;
            LoadFileSystemInfo();
        }

        public void LoadFileSystemInfo()
        {
            if (string.IsNullOrEmpty(FullPath))
            {
                throw new ArgumentException("FullPath must be set");
            }
            FileInfo f = new FileInfo(FullPath);
            IsSystem = f.Attributes.HasFlag(FileAttributes.System);
            Name = f.Name;
            Directory = f.DirectoryName;
            Extension = f.Extension.ToLower();
            Size = f.Length;
            FileModified = f.LastWriteTime;
            if (Size < (SampleBytesOffset + SampleBytesSize))
            {
                SampleBytesOffset = 0;
                SampleBytesSize = (int)Size;
            }
        }

        public bool Equals(ZapperFile other)
        {
            if (Object.ReferenceEquals(other, null)) { return false; }
            if (Object.ReferenceEquals(this, other)) { return true; }
            return
                FullPath == other.FullPath
                && Size == other.Size
                && Extension == other.Extension
                && Name == other.Name
                && Directory == other.Directory
                && FileModified == other.FileModified
                && ContentHash == ContentHash;
        }

        public bool EqualsIgnoreHash(ZapperFile other)
        {
            if (Object.ReferenceEquals(other, null)) { return false; }
            if (Object.ReferenceEquals(this, other)) { return true; }

            return
                FullPath == other.FullPath
                && Size == other.Size
                && Extension == other.Extension
                && Name == other.Name
                && Directory == other.Directory
                && FileModified == other.FileModified;
        }
    }
}