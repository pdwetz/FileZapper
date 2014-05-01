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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileZapper.Core.Data;
using FileZapper.Core.Utilities;
using log4net;

namespace FileZapper.Core.Engine
{
    public class PhaseCalculateHashes : IZapperPhase
    {
        private readonly ILog _log = LogManager.GetLogger("PhaseCalculateHashes");

        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; }
        public bool IsInitialPhase { get; set; }

        public PhaseCalculateHashes()
        {
            Name = "Calculate hashes (this may take awhile)";
        }

        public void Process()
        {
            var possibleDupes = 
                (from z in ZapperProcessor.ZapperFiles.Values
                group z by new { z.Size, z.Extension } into g
                select new { ContentHash = g.Key, Count = g.Count(), Files = g })
                .Where(x => x.Count > 1);

            // TODO Test perf of parallelism here; since it's I/O bound, should we make it more aware of different disks being checked?

            // Parallelism explicitly limited, as default setting was causing hundreds of threads to be created and killed machine performance.
            // Instead, we're simply using the logical processor count (including physical cores and hyperthreading)

            // TODO Test partitioning
            Parallel.ForEach(possibleDupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupeGroup =>
            //Parallel.ForEach(Partitioner.Create(files, EnumerablePartitionerOptions.NoBuffering), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, hashfile =>
            {
                // TODO Pulled logic from removal phase, which had issues trying to parallelize this inner loop along with the outer one
                foreach (var zfile in dupeGroup.Files)
                {
                    Hashify(zfile);
                }
            });
        }

        public static async Task<string> CalculateMD5Hash(string sFilePath)
        {
            using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
            {
                byte[] hashvalue;
                // TODO Test perf of different buffer sizes, particularly for different file sizes (e.g. 10kb, 150kb, 500kb, 1mb, 5mb, 20mb, 50mb, 150mb, 500mb, 1gb, 2gb)
                using (var stream = new BufferedStream(File.OpenRead(sFilePath), 1200000))
                {
                    // TODO Verify that async change is necessary (or maybe should be on buffered file load instead, with the following line removed?)
                    await stream.FlushAsync();
                    hashvalue = hasher.ComputeHash(stream);
                }
                return System.BitConverter.ToString(hashvalue);
            }
        }

        public async void Hashify(ZapperFile zfile)
        {
            if (!string.IsNullOrWhiteSpace(zfile.ContentHash))
            {
                return;
            }
            try
            {
                zfile.LoadFileSystemInfo();
                
                var hashtimer = Stopwatch.StartNew();
                // TODO Do timing differences between Crc32 and MD5
                zfile.ContentHash = await CalculateMD5Hash(zfile.FullPath);
               
                hashtimer.Stop();
                zfile.HashTime = hashtimer.ElapsedMilliseconds;

                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
            catch (Exception ex)
            {
                Exceptioneer.Log(_log, ex, "Due to error, file tagged with INVALID content hash: " + zfile.FullPath);
                zfile.ContentHash = "INVALID";
                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
        }
    }
}