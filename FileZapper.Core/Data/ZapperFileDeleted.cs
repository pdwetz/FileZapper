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
using System;

namespace FileZapper.Core.Data
{
    public class ZapperFileDeleted : ZapperFile
    {
        public DateTime DeletedDate { get; set; }
        public Guid DeletedSessionId { get; set; }

        public ZapperFileDeleted(ZapperFile zfile, Guid sessionId)
        {
            DeletedDate = DateTime.Now;
            DeletedSessionId = sessionId;
            ContentHash = zfile.ContentHash;
            Directory = zfile.Directory;
            Extension = zfile.Extension;
            FileModified = zfile.FileModified;
            FullPath = zfile.FullPath;
            HashTime = zfile.HashTime;
            IsSystem = zfile.IsSystem;
            Name = zfile.Name;
            Score = zfile.Score;
            Size = zfile.Size;
        }
    }
}