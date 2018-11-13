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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileZapper.Core.Data;
using FileZapper.Core.Utilities;
using Serilog;

namespace FileZapper.Core.Engine
{
    public class PhaseCalculateHashes : IZapperPhase
    {
        public ZapperProcessor ZapperProcessor { get; set; }
        public int PhaseOrder { get; set; }
        public string Name { get; set; } = "Calculate content hashes (this may take awhile)";
        public bool IsInitialPhase { get; set; }

        private readonly ILogger _log;

        public PhaseCalculateHashes()
        {
            _log = Log.ForContext<PhaseCalculateHashes>();
        }

        public void Process()
        {
            _log.Information(Name);
            // TODO Test perf of parallelism here; since it's I/O bound, should we make it more aware of different disks being checked?
            var possibleDupes = 
                (from z in ZapperProcessor.ZapperFiles.Values
                where z.SampleHash != null
                group z by new { z.Size, z.Extension, z.SampleHash } into g
                select new { ContentHash = g.Key, Count = g.Count(), Files = g })
                .Where(x => x.Count > 1);

            // Parallelism explicitly limited, as default setting was causing hundreds of threads to be created and killed machine performance.
            // Instead, we're simply using the logical processor count (including physical cores and hyperthreading)

            // TODO Test partitioning
            Parallel.ForEach(possibleDupes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, dupeGroup =>
            //Parallel.ForEach(Partitioner.Create(files, EnumerablePartitionerOptions.NoBuffering), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, hashfile =>
            {
                foreach (var zfile in dupeGroup.Files)
                {
                    Hashify(zfile);
                }
            });
        }

        public static async Task<string> CalculateMD5Hash(string filePath, int bufferSize = 1_200_000)
        {
            using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
            {
                byte[] hashvalue;
                using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
                {
                    await stream.FlushAsync();
                    hashvalue = hasher.ComputeHash(stream);
                }
                return BitConverter.ToString(hashvalue);
            }
        }

        public static async Task<string> CalculateCrcHash(string filePath, int bufferSize = 1_200_000)
        {
            using (Crc32 hasher = new Crc32())
            {
                byte[] hashvalue;
                using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
                {
                    await stream.FlushAsync();
                    hashvalue = hasher.ComputeHash(stream);
                }
                return BitConverter.ToString(hashvalue);
            }
        }

        public static async Task<string> CalculateFarmhashAsync(string filePath)
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            return Farmhash.Sharp.Farmhash.Hash64(bytes, bytes.Length).ToString();
        }

        public static string CalculateFarmhash(string filePath)
        {
            // Can't use Span in async methods
            ReadOnlySpan<byte> sp = File.ReadAllBytes(filePath);
            return Farmhash.Sharp.Farmhash.Hash64(sp).ToString();
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
                switch (ZapperProcessor.Settings.Hasher)
                {
                    case "MD5":
                        zfile.ContentHash = await CalculateMD5Hash(zfile.FullPath);
                        break;
                    case "CRC":
                        zfile.ContentHash = await CalculateCrcHash(zfile.FullPath);
                        break;
                    default:
                        zfile.ContentHash = await CalculateFarmhashAsync(zfile.FullPath);
                        break;
                }
                hashtimer.Stop();
                zfile.HashTime = hashtimer.ElapsedMilliseconds;
                _log.Verbose("File hashed {@Zfile}", zfile);
                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Due to error, file tagged with INVALID content hash: {FullPath}", zfile.FullPath);
                zfile.ContentHash = "INVALID";
                if (!ZapperProcessor.ZapperFiles.TryUpdate(zfile.FullPath, zfile, zfile))
                {
                    throw new FileZapperUpdateDictionaryFailureException("ZapperFiles", zfile.FullPath);
                }
            }
        }
    }
}