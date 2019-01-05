using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChannelsPlayground.Benchmark
{
    public class FixedThreadPoolScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
        private volatile Thread[] _threads;

        public FixedThreadPoolScheduler(int threadCount)
        {
            _threads = new Thread[threadCount];
            for (var i = 0; i < _threads.Length; i++)
            {
                var t = new Thread(() =>
                {
                    foreach (var task in _tasks.GetConsumingEnumerable())
                    {
                        TryExecuteTask(task);
                    }
                });

                t.Start();
                _threads[i] = t;
            }
        }

        public void Dispose()
        {
            _tasks.CompleteAdding();
            _threads = null;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            var threads = _threads ?? throw new ObjectDisposedException(ToString());
            var ct = Thread.CurrentThread;

            for (var i = 0; i < threads.Length; i++)
            {
                if (ct == threads[i])
                {
                    return TryExecuteTask(task);
                }
            }

            return false;
        }
    }
}