using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsPlayground.Benchmark
{
    public class Subscriber
    {
        private readonly ChannelReader<int> _reader;

        public Subscriber(ChannelReader<int> reader)
        {
            _reader = reader;
        }

        public async Task<int> SubscribeAsync(CancellationToken cancellationToken = default)
        {
            var reader = _reader;
            var i = 0;
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out _))
                {
                    i++;
                }
            }

            return i;
        }
    }
}