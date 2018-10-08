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
using CommandLine;
using FileZapper.Core;
using FileZapper.Core.Data;

namespace FileZapper
{
    public class ProgramOptions : FileZapperSettings
    {
        public const int DefaultRootScore = 900000;
        public const int DefaultRootScoreDelta = 100000;

        [Option('f', "folders", Required = true, HelpText = "Folders to be processed.")]
        public IEnumerable<string> Folders { get; set; }

        [Option('s', "scoring", Default = false, Required = false, HelpText = "Scoring same for all root folders")]
        public bool IsRootFolderEqualScoring { get; set; }

        [Option('h', "hasher", Default = "Farmhash", Required = false, HelpText = "Hasher to use")]
        public override string Hasher { get; set; }

        [Option('i', "ignorehierarchy", Required = false, HelpText = "Dupe check ignores folder hierarchy.")]
        public override bool DupeCheckIgnoresHierarchy { get; set; }

        [Option('b', "ignorebelow", Default = 0, Required = false, HelpText = "Ignore files below this value (bytes)")]
        public override long IgnoreFilesBelowBytes { get; set; }

        [Option('o', "ignoreover", Default = long.MaxValue, Required = false, HelpText = "Ignore files over this value (bytes)")]
        public override long IgnoreFilesOverBytes { get; set; }

        [Option('x', "skippedext", Required = false, HelpText = "Skipped file extensions.")]
        public override IEnumerable<string> SkippedExtensions { get; set; }

        [Option('y', "unwantedext", Required = false, HelpText = "Unwanted file extensions to delete (exact match only).")]
        public override IEnumerable<string> UnwantedExtensions { get; set; }

        [Option('z', "unwantedfolder", Required = false, HelpText = "Unwanted folder names to delete (exact match only).")]
        public override IEnumerable<string> UnwantedFolders { get; set; }

        [Option('p', "pause", Required = false, HelpText = "Pause on program completion.")]
        public bool PauseAtCompletion { get; set; }

        public void Init()
        {
            if (SkippedExtensions == null)
            {
                SkippedExtensions = new string[] { };
            }
            if (UnwantedExtensions == null)
            {
                UnwantedExtensions = new string[] { };
            }
            if (UnwantedFolders == null)
            {
                UnwantedFolders = new string[] { };
            }
            RootFolders = new List<ZapperFolder>();
            foreach (var f in Folders)
            {
                RootFolders.Add(new ZapperFolder
                {
                    FullPath = f,
                    Priority = IsRootFolderEqualScoring ? DefaultRootScore : DefaultRootScore - (RootFolders.Count * DefaultRootScoreDelta)
                });
            }
        }
    }
}