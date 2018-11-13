using BenchmarkDotNet.Running;

namespace FileZapper.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Hashmarks>();
        }
    }
}