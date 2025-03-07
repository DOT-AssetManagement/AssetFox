using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.WorkQueue;
using BridgeCareCore.Controllers.BaseController;
using BridgeCareCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeCareCoreTests.Tests.Integration
{
    public static class ServiceProviderExtensions
    {
        public static async Task<IWorkStarter> DequeueAndCompleteSequentialWorkQueueTask(this IServiceProvider serviceProvider)
        {
            var workQueue = serviceProvider.GetService(typeof(SequentialWorkQueue<WorkQueueMetadata>)) as SequentialWorkQueue<WorkQueueMetadata>;
            var cancellationToken = new CancellationToken();
            var task = workQueue.Dequeue(cancellationToken);
            var workStarter = await task;
            workStarter.StartWork(serviceProvider);
            return workStarter;
        }

        // WJPRQ This method was taken from the branch that was rejected by
        // both Jake and Lax. It does allow some tests to pass that
        // otherwise would not. If y'all don't want it, it might be possible
        // to test a different portion of the code in those tests. I could look into that.
        public static async Task<IWorkStarter> DequeueAndCompleteFastWorkQueueTask(this IServiceProvider serviceProvider)
        {
            var workQueue = serviceProvider.GetService(typeof(FastSequentialworkQueue<WorkQueueMetadata>)) as FastSequentialworkQueue<WorkQueueMetadata>;
            var cancellationToken = new CancellationToken();
            var task = workQueue.Dequeue(cancellationToken);
            var workStarter = await task;
            workStarter.StartWork(serviceProvider);
            return workStarter;
        }

        public static async Task<IWorkStarter> DequeueFastWorkQueueTask(this IServiceProvider serviceProvider)
        {
            var workQueue = serviceProvider.GetService(typeof(FastSequentialworkQueue<WorkQueueMetadata>)) as FastSequentialworkQueue<WorkQueueMetadata>;
            var cancellationToken = new CancellationToken();
            var task = workQueue.Dequeue(cancellationToken);
            var workStarter = await task;
            return workStarter;
        }
        public static TController GetControllerWithUnifiedHttpContext<TController>(this IServiceProvider serviceProvider)
            where TController : BridgeCareCoreBaseController
        {
            var controller = serviceProvider.GetRequiredService<TController>();
            var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            controller.ControllerContext.HttpContext = contextAccessor.HttpContext;
            return controller;
        }
    }
}
