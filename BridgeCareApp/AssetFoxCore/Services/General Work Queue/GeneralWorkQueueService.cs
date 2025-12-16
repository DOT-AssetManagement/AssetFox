using System;
using System.Data;
using System.Linq;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.WorkQueue;
using BridgeCareCore.Interfaces;
using BridgeCareCore.Models;

namespace BridgeCareCore.Services
{
    public class GeneralWorkQueueService : IGeneralWorkQueueService
    {
        private readonly SequentialWorkQueue<WorkQueueMetadata> _sequentialWorkQueue;
        private readonly FastSequentialworkQueue<WorkQueueMetadata> _fastQueue;
        private readonly HiddenUploadQueue<WorkQueueMetadata> _hiddenUploadQueue;

        public GeneralWorkQueueService(SequentialWorkQueue<WorkQueueMetadata> sequentialWorkQueue, FastSequentialworkQueue<WorkQueueMetadata> fastQueue, HiddenUploadQueue<WorkQueueMetadata> hiddenUploadQueue)
        {
            _sequentialWorkQueue = sequentialWorkQueue ?? throw new ArgumentNullException(nameof(sequentialWorkQueue));
            _fastQueue = fastQueue ?? throw new ArgumentNullException(nameof(fastQueue));
            _hiddenUploadQueue = hiddenUploadQueue ?? throw new ArgumentNullException( nameof(hiddenUploadQueue));
        }

        public IQueuedWorkHandle<WorkQueueMetadata> CreateAndRun(IWorkSpecification<WorkQueueMetadata> workItem)
        {
            _sequentialWorkQueue.Enqueue(workItem, out var workHandle).Wait();
            return workHandle;
        }

        public bool Cancel(string workId)
        {
            return _sequentialWorkQueue.Cancel(workId);
        }

        public IQueuedWorkHandle<WorkQueueMetadata> CreateAndRunInFastQueue(IWorkSpecification<WorkQueueMetadata> workItem)
        {
            _fastQueue.Enqueue(workItem, out var workHandle).Wait();
            return workHandle;
        }

        public bool CancelInFastQueue(string workId)
        {
            return _fastQueue.Cancel(workId);
        }

        public IQueuedWorkHandle<WorkQueueMetadata> CreateAndRunInHiddenUploadQueue(IWorkSpecification<WorkQueueMetadata> workItem)
        {
            _hiddenUploadQueue.Enqueue(workItem, out var workHandle).Wait();
            return workHandle;
        }

        public bool CancelInHiddenUploadQueue(string workId)
        {
            return _hiddenUploadQueue.Cancel(workId);
        }
    }
}
