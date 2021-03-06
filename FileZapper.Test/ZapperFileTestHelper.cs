﻿/*
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
using System.Reflection;
using FileZapper.Core.Data;

namespace FileZapper.Test
{
    public static class ZapperFileTestHelper
    {
        public const string FillerText = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\n";
        public const string AltFillerText = "Zim zam a dap to tip. Zoom zip, a boom bip. Nada foo bar ah me to doom. Nah do goo pah be rema, zay kew quin hew. Trim sach min wrun. Priny hun foos be tix wun, nie fut as drun pool ret unvon cin. Gree fin hun tintu. Frew sa lin unt vip trebl saed carni jin pog un traem vin asdrtync. Hun in tun run eun aun in. Drun a ne app pro mainwin wern add loops quiy drux. Seeb han chu ke triv bee hi lik paj frim, dree cripfug nium tree add see nah me.\n";

        public static ZapperFolder GetTestFileRoot()
        {
            string folderPath = SetupFolder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles");
            return new ZapperFolder { FullPath = folderPath };
        }

        public static ZapperFolder GetTestFileSubfolder(string name)
        {
            string sParentPath = GetTestFileRoot().FullPath;
            string folderPath = SetupFolder(sParentPath, name);
            return new ZapperFolder { FullPath = folderPath };
        }

        public static ZapperFolder GetTestFileSubfolder(string parentPath, string name)
        {
            string folderPath = SetupFolder(parentPath, name);
            return new ZapperFolder { FullPath = folderPath };
        }

        public static string SetupFolder(string parentPath, string name)
        {
            string folderPath = Path.Combine(parentPath, name);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }

        public static void CreateTextFile(string filePath, long fillerlines, string fillerText = FillerText)
        {
            if (File.Exists(filePath))
            {
                return;
            }
            using (var stream = File.CreateText(filePath))
            {
                for (long i = 0; i < fillerlines; i++)
                {
                    stream.Write(fillerText);
                }
                stream.Flush();
            }
        }
    }
}