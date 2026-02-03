using System;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFoxCore.Models;

namespace AssetFoxCore.Interfaces
{
    public interface IWorkQueueService
    {
        PagingPageModel<QueuedWorkDTO> GetWorkQueuePage(PagingRequestModel<QueuedWorkDTO> request);
        PagingPageModel<QueuedWorkDTO> GetFastWorkQueuePage(PagingRequestModel<QueuedWorkDTO> request);
        QueuedWorkDTO GetQueuedWorkByWorkId(string workId);
        QueuedWorkDTO GetFastQueuedWorkByWorkId(string workId);
        QueuedWorkDTO GetHiddenUploadQueuedWorkByWorkId(string workId);
        QueuedWorkDTO GetQueuedWorkByDomainIdAndWorkType(Guid domainId, WorkType workType);
        QueuedWorkDTO GetFastQueuedWorkByDomainIdAndWorkType(Guid domainId, WorkType workType);
        QueuedWorkDTO GetHiddenUploadQueuedWorkByDomainIdAndWorkType(Guid domainId, WorkType workType);
        QueuedWorkDTO GetQueuedWorkByWorkType(WorkType workType);
        QueuedWorkDTO GetFastQueuedWorkByWorkType(WorkType workType);
        QueuedWorkDTO GetHiddeenUploadQueuedWorkByWorkType(WorkType workType);
    }
}
