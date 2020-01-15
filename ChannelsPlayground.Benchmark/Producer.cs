using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace ChannelsPlayground.Benchmark
{
    public class Producer
    {
        private readonly int _itemsToProduce;
        private readonly ChannelWriter<DataTransferObject> _writer;
        private readonly ObjectPool<DataTransferObject> _pool;

        public Producer(ChannelWriter<DataTransferObject> writer, ObjectPool<DataTransferObject> pool,
            int itemsToProduce)
        {
            _writer = writer;
            _pool = pool;
            _itemsToProduce = itemsToProduce;
        }

        public async Task ProduceAsync(CancellationToken cancellationToken = default)
        {
            var itemsToProduce = _itemsToProduce;
            var writer = _writer;

            var i = 0;
            var dto = _pool.Get();

            while (i < itemsToProduce && await writer.WaitToWriteAsync(cancellationToken))
            {
                while (i < itemsToProduce && writer.TryWrite(dto))
                {
                    i++;
                    dto = _pool.Get();
                }
            }
            
            _pool.Return(dto);
        }
    }
}