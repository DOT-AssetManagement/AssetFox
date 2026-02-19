using System;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public interface IViewerDataService
    {
        ViewerMetadataDTO GetMetadata(Guid simulationId);
        PagedViewerResultDTO GetComponentData(Guid schemaId, string componentId, ViewerDataRequestDTO request);
    }
}
