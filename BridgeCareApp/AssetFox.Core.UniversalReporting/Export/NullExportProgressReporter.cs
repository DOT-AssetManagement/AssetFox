namespace AssetFox.Core.UniversalReporting.Export;

public sealed class NullExportProgressReporter : IExportProgressReporter
{
    public static readonly NullExportProgressReporter Instance = new();

    private NullExportProgressReporter()
    {
    }

    public Task ReportAsync(string status, CancellationToken cancellationToken) => Task.CompletedTask;
}
