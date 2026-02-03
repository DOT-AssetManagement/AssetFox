using AssetFox.Core.WorkQueue;
using AssetFoxCore.Models;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface IGeneralWorkQueueService
    {
        IQueuedWorkHandle<WorkQueueMetadata> CreateAndRun(IWorkSpecification<WorkQueueMetadata> workItem);
        IQueuedWorkHandle<WorkQueueMetadata> CreateAndRunInFastQueue(IWorkSpecification<WorkQueueMetadata> workItem);
        IQueuedWorkHandle<WorkQueueMetadata> CreateAndRunInHiddenUploadQueue(IWorkSpecification<WorkQueueMetadata> workItem);

        bool Cancel(string simulationId);
        bool CancelInFastQueue(string workId);
        bool CancelInHiddenUploadQueue(string workId);
    }
}
