using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCoreTests.Tests.Integration
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

        public static async Task<IWorkStarter> DequeueAndCompleteFastWorkQueueTask(this IServiceProvider serviceProvider)
        {
            var workQueue = serviceProvider.GetService(typeof(FastSequentialworkQueue<WorkQueueMetadata>)) as FastSequentialworkQueue<WorkQueueMetadata>;
            var cancellationToken = new CancellationToken();
            var task = workQueue.Dequeue(cancellationToken);
            var workStarter = await task;
            workStarter.StartWork(serviceProvider);
            return workStarter;
        }

        public static async Task<IWorkStarter> DequeueAndCompleteHiddenUploadQueueTask(this IServiceProvider serviceProvider)
        {
            var workQueue = serviceProvider.GetService(typeof(HiddenUploadQueue<WorkQueueMetadata>)) as HiddenUploadQueue<WorkQueueMetadata>;
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
            where TController : AssetFoxCoreBaseController
        {
            var controller = serviceProvider.GetRequiredService<TController>();
            var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            controller.ControllerContext.HttpContext = contextAccessor.HttpContext;
            return controller;
        }
    }
}
