using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace ChannelsPlayground.Benchmark
{
    [Config(typeof(Config))]
    public class ChannelsBenchmark
    {
        public enum Cardinality
        {
            Single,
            Multi
        }

        public enum ChannelType
        {
            BoundedWait,
            Unbounded
        }

        private const int Single = 1;
        private const int Multi = 10;
        private const int Capacity = 1_000_000;

        [Params(Cardinality.Multi, Cardinality.Single)]
        public Cardinality PublisherCardinality { get; set; }

        [Params(Cardinality.Multi, Cardinality.Single)]
        public Cardinality SubscriberCardinality { get; set; }

        [Params(ChannelType.BoundedWait, ChannelType.Unbounded)]
        public ChannelType Type { get; set; }

        [Params(true, false)] 
        public bool AllowSyncContinuations { get; set; }

        private FixedThreadPoolScheduler SubscriberScheduler { get; set; }
        private FixedThreadPoolScheduler PublisherScheduler { get; set; }

        private int SubscriberCount => SubscriberCardinality == Cardinality.Single ? Single : Multi;

        private int ProducerCount => PublisherCardinality == Cardinality.Single ? Single : Multi;

        [GlobalSetup]
        public void Setup()
        {
            SubscriberScheduler = new FixedThreadPoolScheduler(SubscriberCount);
            PublisherScheduler = new FixedThreadPoolScheduler(ProducerCount);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            SubscriberScheduler.Dispose();
            PublisherScheduler.Dispose();
        }

        [Benchmark]
        public async Task<int> ChannelPerf()
        {
            var channel = CreateChannel();
            var itemsToProduce = Capacity / ProducerCount;

            var producerFactory = new ProducerFactory(channel, ProducerCount, itemsToProduce, PublisherScheduler);
            var subscriberFactory = new SubscriberFactory(channel, SubscriberCount, SubscriberScheduler);

            var prodThread = producerFactory.StartProducersAsync();
            var subsThread = subscriberFactory.StartSubscribersAsync();
            
            await Task.WhenAll(prodThread, subsThread);

            return subsThread.Result;
        }

        private Channel<int> CreateChannel()
        {
            switch (Type)
            {
                case ChannelType.BoundedWait:
                    return Channel.CreateBounded<int>(new BoundedChannelOptions(Capacity)
                    {
                        AllowSynchronousContinuations = AllowSyncContinuations,
                        FullMode = BoundedChannelFullMode.Wait,
                        SingleReader = SubscriberCardinality == Cardinality.Single,
                        SingleWriter = PublisherCardinality == Cardinality.Single
                    });
                case ChannelType.Unbounded:
                    return Channel.CreateUnbounded<int>(new UnboundedChannelOptions
                    {
                        AllowSynchronousContinuations = AllowSyncContinuations,
                        SingleReader = SubscriberCardinality == Cardinality.Single,
                        SingleWriter = PublisherCardinality == Cardinality.Single
                    });
                default:
                    throw new InvalidOperationException();
            }
        }

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(StatisticColumn.OperationsPerSecond);
            }
        }
    }
}
