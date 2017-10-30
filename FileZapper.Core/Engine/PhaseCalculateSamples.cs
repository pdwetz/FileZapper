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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileZapper.Core.Data;
using FileZapper.Core.Utilities;
using log4net;

namespace FileZapper.Core.Engine
{
    public class PhaseCalculateSamples : IZapperPhase
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PhaseCalculateSamples));

        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseCalculateSamples()
        {
            Name = "Calculate sample hashes";
        }

        public void Process()
        {
            _log.Info(Name);
            if (ZapperProcessor.Settings.DupeCheckIgnoresHierarchy)
            {
                var possibleDupes =
                    (from z in ZapperProcessor.ZapperFiles.Values
                     group z by new { z.Size, z.Extension } into g
                     select new { ContentHash = g.Key, Count = g.Count(), Files = g })
                    .Where(x => x.Count > 1);

                Parallel.ForEach(possibleDupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupeGroup =>
                {
                    foreach (var zfile in dupeGroup.Files)
                    {
                        Hashify(zfile);
                    }
                });
            }
            else
            {
                var possibleDupes =
                    (from z in ZapperProcessor.ZapperFiles.Values
                     group z by new { z.Directory, z.Size } into g
                     select new { ContentHash = g.Key, Count = g.Count(), Files = g })
                    .Where(x => x.Count > 1);

                Parallel.ForEach(possibleDupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupeGroup =>
                {
                    foreach (var zfile in dupeGroup.Files)
                    {
                        Hashify(zfile);
                    }
                });
            }
        }

        public void Hashify(ZapperFile zfile)
        {
            if (!string.IsNullOrWhiteSpace(zfile.ContentHash))
            {
                return;
            }
            try
            {
                zfile.LoadFileSystemInfo();
                var buffer = new byte[zfile.SampleBytesSize];
                using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
                {
                    byte[] hashvalue;
                    using (var stream = File.OpenRead(zfile.FullPath))
                    {
                        stream.Seek(zfile.SampleBytesOffset, SeekOrigin.Begin);
                        stream.Read(buffer, 0, zfile.SampleBytesSize);
                        hashvalue = hasher.ComputeHash(buffer);
                    }
                    zfile.SampleHash = BitConverter.ToString(hashvalue);
                }
                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(_log, ex, $"Due to error, file tagged with INVALID sample hash: {zfile.FullPath}");
                zfile.SampleHash = "INVALID";
                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
        }
    }
}