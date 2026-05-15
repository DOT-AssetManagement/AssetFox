using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class SequentialWorkQueueTests
    {
        [Fact]
        public async Task items_execute_in_the_order_they_were_added()
        {
            SequentialWorkQueue<WorkQueueMetadata> queue = new();
            List<int> taskEffects = new();
            
            await queue.Enqueue(new TestWorkItem(1, 0, taskEffects), out _);
            await queue.Enqueue(new TestWorkItem(2, 1000, taskEffects), out _);
            await queue.Enqueue(new TestWorkItem(3, 0, taskEffects), out _);
            await queue.Enqueue(new TestWorkItem(4, 1000, taskEffects), out _);
            await queue.Enqueue(new TestWorkItem(5, 0, taskEffects), out _);

            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var workStarter = await queue.Dequeue(cts.Token);
                    workStarter.StartWork(null);
                }
            }
            catch (OperationCanceledException)
            {
            }

            Assert.Equal(Enumerable.Range(1, 5), taskEffects);
        }

        private record TestWorkItem(int Id, int MsDelay, List<int> WorkTarget) : IWorkSpecification<WorkQueueMetadata>
        {
            public string WorkId { get; } = Id.ToString();

            public string UserId => "";

            public WorkQueueMetadata Metadata => throw new NotImplementedException();

            public string WorkDescription => "";

            public string WorkName => throw new NotImplementedException();

            public void DoWork(IServiceProvider serviceProvider, Action<string> updateStatus, CancellationToken cancellationToken)
            {
                WorkTarget.Add(Id);
                Task.Delay(MsDelay).Wait();
            }

            public void OnCompletion(IServiceProvider serviceProvider) {}
            public void OnFault(IServiceProvider serviceProvider, Exception? e) => throw new NotImplementedException();
            public void OnUpdate(IServiceProvider serviceProvider) { }
        }
    }
}
