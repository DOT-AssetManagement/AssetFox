using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Viewer
{
    public class ViewerDataService : IViewerDataService
    {
        private readonly UnitOfDataPersistenceWork _unitOfWork;
        private readonly IUniversalQueryEngine _queryEngine;

        public ViewerDataService(
            UnitOfDataPersistenceWork unitOfWork,
            IUniversalQueryEngine queryEngine)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
        }

        public ViewerMetadataDTO GetMetadata(Guid simulationId)
        {
            var context = _unitOfWork.Context;

            var simOutput = context.SimulationOutput
                .AsNoTracking()
                .FirstOrDefault(so => so.SimulationId == simulationId);

            if (simOutput == null)
            {
                return new ViewerMetadataDTO
                {
                    SimulationId = simulationId,
                    HasOutput = false
                };
            }

            var runId = simOutput.RunId;

            var simulation = context.Simulation
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == simulationId);

            var years = context.SimulationYearDetail
                .AsNoTracking()
                .Where(y => y.RunId == runId)
                .OrderBy(y => y.Year)
                .Select(y => y.Year)
                .ToList();

            var assetCount = context.AssetSummaryDetail
                .AsNoTracking()
                .Where(a => a.RunId == runId)
                .Count();

            var sampleAssetId = context.AssetSummaryDetail
                .AsNoTracking()
                .Where(a => a.RunId == runId)
                .Select(a => a.Id)
                .FirstOrDefault();

            var availableAttributes = new List<string>();
            if (sampleAssetId != Guid.Empty)
            {
                var attributeIds = context.AssetSummaryDetailValueIntId
                    .AsNoTracking()
                    .Where(v => v.RunId == runId && v.AssetSummaryDetailId == sampleAssetId)
                    .Select(v => v.AttributeId)
                    .Distinct()
                    .ToList();

                var allAttributes = _unitOfWork.AttributeRepo.GetAllAttributesAbbreviated();
                availableAttributes = allAttributes
                    .Where(a => attributeIds.Contains(a.Id))
                    .Select(a => a.Name)
                    .OrderBy(n => n)
                    .ToList();
            }

            return new ViewerMetadataDTO
            {
                SimulationId = simulationId,
                SimulationName = simulation?.Name ?? "Unknown",
                RunId = runId,
                Years = years,
                AssetCount = assetCount,
                AvailableAttributes = availableAttributes,
                HasOutput = true
            };
        }

        public PagedViewerResultDTO GetComponentData(Guid schemaId, string componentId, ViewerDataRequestDTO request)
        {
            var schemaDto = _unitOfWork.ReportSchemaRepo.Get(schemaId);
            if (schemaDto == null)
            {
                throw new InvalidOperationException($"Schema {schemaId} not found.");
            }

            var schema = SchemaParser.Parse(schemaDto.SchemaJson);
            var component = schema.AllComponents.FirstOrDefault(c =>
                string.Equals(c.Id, componentId, StringComparison.OrdinalIgnoreCase));

            if (component == null)
            {
                throw new InvalidOperationException($"Component {componentId} not found in schema.");
            }

            var runId = ResolveRunId(request.SimulationId);
            if (runId == 0)
            {
                return EmptyResult(request);
            }

            var viewerRequest = ViewerDtoMapper.FromDto(request);
            var result = _queryEngine.ExecutePagedAsync(component, viewerRequest, runId, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            return ViewerDtoMapper.ToDto(result);
        }

        private int ResolveRunId(Guid simulationId)
        {
            return _unitOfWork.Context.SimulationOutput
                .Where(so => so.SimulationId == simulationId)
                .Select(so => so.RunId)
                .FirstOrDefault();
        }

        private static PagedViewerResultDTO EmptyResult(ViewerDataRequestDTO request)
        {
            return new PagedViewerResultDTO
            {
                Columns = new List<ViewerColumnDTO>(),
                Rows = new List<List<object>>(),
                TotalRowCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
