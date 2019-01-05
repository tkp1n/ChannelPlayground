using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

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

        public async Task<int> StartSubscribersAsync(CancellationToken cancellationToken = default)
        {
            var subscribers = new Task<int>[_subscriberCount];
            for (var i = 0; i < subscribers.Length; i++)
            {
                var subscriberTask = new Subscriber(_reader).SubscribeAsync(cancellationToken);
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