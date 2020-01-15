using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace ChannelsPlayground.Benchmark
{
    public class ProducerFactory
    {
        private readonly int _itemsToProduce;
        private readonly int _producerCount;
        private readonly TaskScheduler _scheduler;
        private readonly ChannelWriter<DataTransferObject> _writer;
        private readonly ObjectPool<DataTransferObject> _pool;

        public ProducerFactory(ChannelWriter<DataTransferObject> writer,
            ObjectPool<DataTransferObject> pool,
            int producerCount,
            int itemsToProduce, TaskScheduler scheduler)
        {
            _writer = writer;
            _pool = pool;
            _producerCount = producerCount;
            _itemsToProduce = itemsToProduce;
            _scheduler = scheduler;
        }

        public async Task StartProducersAsync(CancellationToken cancellationToken = default)
        {
            var producers = new Task[_producerCount];
            for (var i = 0; i < producers.Length; i++)
            {
                var producerTask = new Producer(_writer, _pool, _itemsToProduce).ProduceAsync(cancellationToken);
                producers[i] = Task.Factory.StartNew(
                    async () => await producerTask,
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach,
                    _scheduler
                ).Unwrap();
            }

            await Task.WhenAll(producers);
            _writer.Complete();
        }
    }
}