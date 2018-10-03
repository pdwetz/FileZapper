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
using FileZapper.Core;
using FileZapper.Core.Engine;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace FileZapper
{
    class Program
    {
        public const string AppName = "FileZapper";

        static void Main(string[] args)
        {
            // TODO Walk through all files and do cleanup, make notes for future changes, etc.

            Console.Title = "FileZapper";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var settings = new FileZapperSettings();
            config.GetSection("FileZapperSettings").Bind(settings);
            // TODO Grab log config via appsettings.json
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("Logs\\log-{Date}.txt", LogEventLevel.Information)
                .CreateLogger();

            var app = new CommandLineApplication
            {
                Name = AppName
            };
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                try
                {
                    Console.WriteLine("FileZapper   Copyright (C) 2018 Peter Wetzel");
                    Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY; for details see license.txt.");
                    ZapperProcessor z = new ZapperProcessor(settings);
                    z.Process();
                    Console.WriteLine($"{DateTime.Now.ToString("HH: mm:ss.fff")}: Done. Press any key to continue.");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    var log = Log.ForContext<ZapperProcessor>();
                    log.Error(ex, ex.Message);
                }
                finally
                {
                    Log.CloseAndFlush();
                }
                return 0;
            });
            app.Execute(args);
        }
    }
}