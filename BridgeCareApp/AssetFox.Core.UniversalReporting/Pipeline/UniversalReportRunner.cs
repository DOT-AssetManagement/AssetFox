using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UniversalReporting.Export;
using AssetFox.Core.UniversalReporting.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Pipeline;

public class UniversalReportRunner
{
    private readonly UnitOfDataPersistenceWork _unitOfWork;
    private readonly IUniversalReportExporter _exporter;

    public UniversalReportRunner(
        UnitOfDataPersistenceWork unitOfWork,
        IUniversalReportExporter exporter)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
    }

    public async Task<string> RunAsync(
        Guid simulationId,
        Guid schemaId,
        CancellationToken cancellationToken,
        IExportProgressReporter? progressReporter = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var schemaDto = _unitOfWork.ReportSchemaRepo.Get(schemaId);
        if (schemaDto == null)
        {
            throw new InvalidOperationException($"Report schema not found with ID {schemaId}");
        }

        var simulationOutput = await _unitOfWork.Context.SimulationOutput
            .AsNoTracking()
            .FirstOrDefaultAsync(output => output.SimulationId == simulationId, cancellationToken);

        if (simulationOutput == null)
        {
            throw new InvalidOperationException($"Simulation output not found for simulation ID {simulationId}");
        }

        var schema = SchemaParser.Parse(schemaDto.SchemaJson);
        return await _exporter.ExportAsync(
            schema,
            simulationId,
            simulationOutput.RunId,
            cancellationToken,
            progressReporter);
    }
}
