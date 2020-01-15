using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.ObjectPool;

namespace ChannelsPlayground.Benchmark
{
    public class SubscriberFactory
    {
        private readonly ChannelReader<DataTransferObject> _reader;
        private readonly ObjectPool<DataTransferObject> _pool;
        private readonly TaskScheduler _scheduler;
        private readonly int _subscriberCount;

        public SubscriberFactory(ChannelReader<DataTransferObject> reader, ObjectPool<DataTransferObject> pool, 
            int subscriberCount, TaskScheduler scheduler)
        {
            _reader = reader;
            _subscriberCount = subscriberCount;
            _scheduler = scheduler;
            _pool = pool;
        }

        public async Task<int> StartSubscribersAsync(CancellationToken cancellationToken = default)
        {
            var subscribers = new Task<int>[_subscriberCount];
            for (var i = 0; i < subscribers.Length; i++)
            {
                var subscriberTask = new Subscriber(_reader, _pool).SubscribeAsync(cancellationToken);
                subscribers[i] = Task.Factory.StartNew(
                    async () => await subscriberTask,
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach,
                    _scheduler
                ).Unwrap();
            }

            await Task.WhenAll(subscribers);

            var sum = 0;
            for (var i = 0; i < subscribers.Length; i++)
            {
                sum += subscribers[i].Result;
            }

            return sum;
        }
    }
}