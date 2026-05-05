using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting;
using AssetFox.Core.Reporting.Interfaces;
using AssetFox.Core.UniversalReporting.Pipeline;

namespace AssetFoxCore.Services.Reporting;

public sealed class UniversalReportFactory : IReportFactory
{
    public const string ReportName = "UniversalReport";

    public string Name => ReportName;

    public IReport Create(
        IUnitOfWork uow,
        ReportIndexDTO results,
        IHubService hubService,
        string suffix = "")
    {
        return new UniversalReportLegacyAdapter(uow, Name, results, hubService);
    }
}
