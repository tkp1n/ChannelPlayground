using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace ChannelsPlayground.Benchmark
{
    public class Subscriber
    {
        private readonly ChannelReader<DataTransferObject> _reader;
        private readonly ObjectPool<DataTransferObject> _pool;

        public Subscriber(ChannelReader<DataTransferObject> reader, ObjectPool<DataTransferObject> pool)
        {
            _reader = reader;
            _pool = pool;
        }

        public async Task<int> SubscribeAsync(CancellationToken cancellationToken = default)
        {
            var reader = _reader;
            var i = 0;
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var dto))
                {
                    _pool.Return(dto);
                    i++;
                }
            }

            return i;
        }
    }
}