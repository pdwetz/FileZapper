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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using FileZapper.Core.Data;
using Serilog;

namespace FileZapper.Core.Engine
{
    public class ZapperProcessor
    {
        public const string LogFilenameSessions = "zappersessions.csv";

        public FileZapperSettings Settings { get; set; }
        public ZapperSession ZapperSession { get { return _zapperSession; } }
        public ConcurrentDictionary<string, ZapperFile> ZapperFiles { get; set; }
        public ConcurrentDictionary<string, ZapperFileDeleted> ZapperFilesDeleted { get; set; }

        private readonly IOrderedEnumerable<IZapperPhase> _phases;
        private readonly ZapperSession _zapperSession = new ZapperSession { Id = Guid.NewGuid() };
        private readonly ILogger _log;

        public ZapperProcessor(FileZapperSettings settings, IList<IZapperPhase> phases = null)
        {
            _log = Log.ForContext<ZapperProcessor>();
            _log.Information("Initializing");

            ZapperFiles = new ConcurrentDictionary<string, ZapperFile>();
            ZapperFilesDeleted = new ConcurrentDictionary<string, ZapperFileDeleted>();
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (phases != null)
            {
                foreach (var phase in phases) { phase.ZapperProcessor = this; }
                _phases = phases.OrderBy(x => x.PhaseOrder);
            }
            else
            {
                List<IZapperPhase> allphases = new List<IZapperPhase>();
                allphases.Add(new PhaseCleanup { PhaseOrder = 1, ZapperProcessor = this, IsInitialPhase = true });
                allphases.Add(new PhaseParseFilesystem { PhaseOrder = 2, ZapperProcessor = this });
                allphases.Add(new PhaseCalculateSamples { PhaseOrder = 3, ZapperProcessor = this });
                allphases.Add(new PhaseCalculateHashes { PhaseOrder = 4, ZapperProcessor = this });
                allphases.Add(new PhaseRemoveDuplicates { PhaseOrder = 5, ZapperProcessor = this });
                _phases = allphases.OrderBy(x => x.PhaseOrder);
            }
        }

        public void StartConsole()
        {
            try
            {
                _log.Verbose("Current configuration settings: {@Settings}", Settings);
                _zapperSession.CurrentPhase = _phases.First(x => x.IsInitialPhase).PhaseOrder;
                Process();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error encountered");
            }
        }

        public void Process()
        {
            try
            {
                _log.Information("Processing");
                _zapperSession.StartDate = DateTime.Now;
                var phases = _phases.Where(x => x.PhaseOrder >= _zapperSession.CurrentPhase);
                foreach (var phase in phases)
                {
                    _log.Information("Phase {CurrentPhase} - {Name}", _zapperSession.CurrentPhase, phase.Name);
                    phase.Process();
                    _zapperSession.PhasesProcessed++;
                    _zapperSession.CurrentPhase++;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error while processing");
            }
            finally
            {
                _log.Information("Done.");
                _zapperSession.EndDate = DateTime.Now;
                _zapperSession.RuntimeMS = (_zapperSession.EndDate.Value - _zapperSession.StartDate).TotalMilliseconds;
                _zapperSession.FilesAdded = ZapperFiles.Count;
                _zapperSession.FilesSampled = ZapperFiles.Values.Count(x => !string.IsNullOrWhiteSpace(x.SampleHash));
                _zapperSession.FilesHashed = ZapperFiles.Values.Count(x => !string.IsNullOrWhiteSpace(x.ContentHash));
                _zapperSession.FilesDeleted = ZapperFilesDeleted.Count;
                _zapperSession.TotalFilesProcessed = _zapperSession.FilesAdded + _zapperSession.FilesDeleted;
                LogResults();
            }
        }

        public string LogResults(string logPath = null)
        {
            if (string.IsNullOrWhiteSpace(logPath))
            {
                logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Results");
            }
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            _log.Information("Logging results to {LogPath}", logPath);
            string filePath = Path.Combine(logPath, LogFilenameSessions);
            bool bSessionFileExists = File.Exists(filePath);
            using (var textWriter = File.AppendText(filePath))
            {
                using (var writer = new CsvWriter(textWriter))
                {
                    if (!bSessionFileExists)
                    {
                        writer.WriteHeader<ZapperSession>();
                    }
                    writer.WriteRecord(_zapperSession);
                }
            }
            if (ZapperFiles.Count > 0)
            {
                filePath = Path.Combine(logPath, $"files-{_zapperSession.Id}.csv");
                using (var textWriter = File.CreateText(filePath))
                {
                    using (var writer = new CsvWriter(textWriter))
                    {
                        writer.WriteRecords(ZapperFiles.Values);
                    }
                }
            }
            if (ZapperFilesDeleted.Count > 0)
            {
                filePath = Path.Combine(logPath, $"deleted-{_zapperSession.Id}.csv");
                using (var textWriter = File.CreateText(filePath))
                {
                    using (var writer = new CsvWriter(textWriter))
                    {
                        writer.WriteRecords(ZapperFilesDeleted.Values);
                    }
                }
            }
            return logPath;
        }
    }
}