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
using System;

namespace FileZapper.Core.Engine
{
    public class FileZapperUpdateDictionaryFailureException : Exception
    {
        public FileZapperUpdateDictionaryFailureException(string dictionaryName, string key)
            : base($"Dictionary {dictionaryName} unable to update key '{key}'")
        { }
    }
}