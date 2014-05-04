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
using System.IO;
using FileZapper.Core.Utilities;
using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace FileZapper.Core.Data
{
    //public class ZapperFileMap : CsvClassMap<ZapperFile>
    //{
    //    public override void CreateMap()
    //    {
    //        Map(m => m.FullPath).Index(0).TypeConverterOption(CultureInfo.InvariantCulture);
    //        Map(m => m.Size).Index(1).TypeConverterOption(NumberStyles.Integer);
    //        Map(m => m.FileModified).Index(2).TypeConverterOption(DateTimeStyles.AssumeLocal);
    //        Map(m => m.HashTime).Index(3).TypeConverterOption(NumberStyles.Integer);
    //        Map(m => m.Score).Index(4).TypeConverterOption(NumberStyles.Integer);
    //        //Map(m => m.SampleHash).Index(zzz).ConvertUsing(row => { return string.IsNullOrWhiteSpace(row.GetField<string>("SampleHash")) ? "No" : "Yes"; });
    //        //Map(m => m.ContentHash).Index(zzz).ConvertUsing(row => { return string.IsNullOrWhiteSpace(row.GetField<string>("ContentHash")) ? "No" : "Yes"; }); 
            
    //    }
    //    /*
    //     public string FullPath { get; set; }
    //    public string Name { get; set; }
    //    public string ContentHash { get; set; }
    //    public string Extension { get; set; }
    //    public string Directory { get; set; }
    //    public long Size { get; set; }
    //    public DateTime? FileModified { get; set; }
    //    public long HashTime { get; set; }
    //    public bool IsSystem { get; set; }
    //    public int Score { get; set; }
    //     */
    //}
}