using AssetFox.Core.DTOs;

namespace AssetFox.Core.UniversalReporting.Viewer;

public interface IViewerDataService
{
    ViewerMetadataDTO GetMetadata(Guid simulationId);
    PagedViewerResultDTO GetComponentData(Guid schemaId, string componentId, ViewerDataRequestDTO request);
}
