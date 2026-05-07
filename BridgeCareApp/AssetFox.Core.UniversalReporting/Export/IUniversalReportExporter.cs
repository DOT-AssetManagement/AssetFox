using AssetFox.Core.UniversalReporting.Schema;

namespace AssetFox.Core.UniversalReporting.Export;

public interface IUniversalReportExporter
{
    Task<string> ExportAsync(
        ReportSchemaDefinition schema,
        Guid simulationId,
        int runId,
        CancellationToken cancellationToken,
        IExportProgressReporter? progressReporter = null);
}
