using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace ChannelsPlayground.Benchmark
{
    internal static class Program
    {
        public static async Task Main()
        {
            /*
            ChannelsBenchmark b = new ChannelsBenchmark();
            b.Setup();
            for (int i = 0; i < 100; i++)
            {
                await b.ChannelPerf();
            }
            b.Cleanup();
            */
            BenchmarkRunner.Run<ChannelsBenchmark>();
        }
    }
}