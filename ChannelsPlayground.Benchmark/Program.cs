using BenchmarkDotNet.Running;

namespace ChannelsPlayground.Benchmark
{
    internal static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<ChannelsBenchmark>();
        }
    }
}