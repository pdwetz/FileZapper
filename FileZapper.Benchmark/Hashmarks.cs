using BenchmarkDotNet.Attributes;
using FileZapper.Core.Engine;
using System.Threading.Tasks;

namespace FileZapper.Benchmark
{
    public class Hashmarks
    {
        [Params("assets/small.txt", "assets/medium.txt")]
        public string FilePath { get; set; }

        [Params(1_200_000)]
        public int BufferSize { get; set; }

        [Benchmark]
        public async Task<string> FarmhashAsync() => await PhaseCalculateHashes.CalculateFarmhashAsync(FilePath);

        [Benchmark]
        public string FarmhashSync() => PhaseCalculateHashes.CalculateFarmhash(FilePath);

        [Benchmark]
        public async Task<string> MD5() => await PhaseCalculateHashes.CalculateMD5Hash(FilePath, BufferSize);

        [Benchmark]
        public async Task<string> Crc() => await PhaseCalculateHashes.CalculateCrcHash(FilePath, BufferSize);
    }
}