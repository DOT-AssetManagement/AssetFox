using AppliedResearchAssociates.iAM.WorkQueue;
using BridgeCareCore.Models;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.Hosting;

namespace BridgeCareCore.Services.General_Work_Queue
{
    public class HiddenUploadQueueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HiddenUploadQueue<WorkQueueMetadata> _sequentialWorkQueue;

        public HiddenUploadQueueBackgroundService(IServiceProvider serviceProvider, HiddenUploadQueue<WorkQueueMetadata> sequentialWorkQueue)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _sequentialWorkQueue = sequentialWorkQueue ?? throw new ArgumentNullException(nameof(sequentialWorkQueue));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workStarter = await _sequentialWorkQueue.Dequeue(stoppingToken);
                workStarter?.StartWork(_serviceProvider);
            }
        }
    }
}
