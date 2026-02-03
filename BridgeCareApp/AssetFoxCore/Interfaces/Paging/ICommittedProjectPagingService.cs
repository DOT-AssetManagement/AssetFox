using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Interfaces
{
    public interface ICommittedProjectPagingService
    {
        PagingPageModel<SectionCommittedProjectDTO> GetCommittedProjectPage(List<SectionCommittedProjectDTO> committedProjects, PagingRequestModel<SectionCommittedProjectDTO> request);

        List<SectionCommittedProjectDTO> GetSyncedDataset(Guid simulationId, PagingSyncModel<SectionCommittedProjectDTO> request);
    }
}
