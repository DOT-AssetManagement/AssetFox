namespace AssetFox.Core.UniversalReporting.Export;

public interface IExportProgressReporter
{
    Task ReportAsync(string status, CancellationToken cancellationToken);
}
