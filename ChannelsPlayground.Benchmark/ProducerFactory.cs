using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsPlayground.Benchmark
{
    public class ProducerFactory
    {
        private readonly int _itemsToProduce;
        private readonly int _producerCount;
        private readonly TaskScheduler _scheduler;
        private readonly ChannelWriter<int> _writer;

        public ProducerFactory(ChannelWriter<int> writer,
            int producerCount,
            int itemsToProduce, TaskScheduler scheduler)
        {
            _writer = writer;
            _producerCount = producerCount;
            _itemsToProduce = itemsToProduce;
            _scheduler = scheduler;
        }

        public async Task StartProducersAsync(CancellationToken cancellationToken = default)
        {
            var producers = new Task[_producerCount];
            for (var i = 0; i < producers.Length; i++)
            {
                var producerTask = new Producer(_writer, _itemsToProduce).ProduceAsync(cancellationToken);
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