using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelsPlayground.Benchmark
{
    public class SubscriberFactory
    {
        private readonly ChannelReader<int> _reader;
        private readonly TaskScheduler _scheduler;
        private readonly int _subscriberCount;

        public SubscriberFactory(ChannelReader<int> reader, int subscriberCount, TaskScheduler scheduler)
        {
            _reader = reader;
            _subscriberCount = subscriberCount;
            _scheduler = scheduler;
        }

        public async Task StartSubscribersAsync(CancellationToken cancellationToken = default)
        {
            var subscribers = new Task[_subscriberCount];
            for (var i = 0; i < subscribers.Length; i++)
            {
                var subscriberTask = new Subscriber(_reader).SubscribeAsync(cancellationToken);
                subscribers[i] = Task.Factory.StartNew(
                    async () => await subscriberTask,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    _scheduler
                ).Unwrap();
            }

            await Task.WhenAll(subscribers);
        }
    }
}