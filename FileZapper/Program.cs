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
using CommandLine;
using FileZapper.Core.Engine;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace FileZapper
{
    class Program
    {
        public const string AppName = "FileZapper";

        static void Main(string[] args)
        {
            // TODO Walk through all files and do cleanup, make notes for future changes, etc.

            Console.Title = "FileZapper";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            Parser.Default.ParseArguments<ProgramOptions>(args)
                .WithParsed<ProgramOptions>(opts => RunOptionsAndReturnExitCode(opts));
        }

        private static void RunOptionsAndReturnExitCode(ProgramOptions options)
        {
            try
            {
                options.Init();
                ZapperProcessor z = new ZapperProcessor(options);
                z.Process();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
                if (options.PauseAtCompletion)
                {
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadLine();
                }
            }
        }
    }
}