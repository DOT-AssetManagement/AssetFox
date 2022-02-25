using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BridgeCareCore.Services;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Services
{
    public class SequentialWorkQueueTests
    {
        [Fact]
        public void items_execute_in_the_order_they_were_added()
        {
            var queue = new SequentialWorkQueue();
            var taskEffects = new List<int>();

            queue.Enqueue(new TestWorkItem(1, 0, taskEffects), out _).Wait();
            queue.Enqueue(new TestWorkItem(2, 1000, taskEffects), out _).Wait();
            queue.Enqueue(new TestWorkItem(3, 0, taskEffects), out _).Wait();
            queue.Enqueue(new TestWorkItem(4, 1000, taskEffects), out _).Wait();
            queue.Enqueue(new TestWorkItem(5, 0, taskEffects), out _).Wait();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var workItem = queue.Dequeue(cts.Token).Result;
                    workItem.DoWork(null);
                }
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            {
            }

            Assert.Equal(Enumerable.Range(1, 5), taskEffects);
        }

        private class TestWorkItem : IWorkItem
        {
            public TestWorkItem(int Id, int MsDelay, List<int> WorkTarget)
            {
                this.Id = Id;
                this.MsDelay = MsDelay;
                this.WorkTarget = WorkTarget;

                WorkId = Id.ToString();
            }

            public int Id { get; }

            public int MsDelay { get; }

            public string WorkId { get; }

            public List<int> WorkTarget { get; }

            public void DoWork(IServiceProvider serviceProvider)
            {
                WorkTarget.Add(Id);
                Task.Delay(MsDelay).Wait();
            }
        }
    }
}
