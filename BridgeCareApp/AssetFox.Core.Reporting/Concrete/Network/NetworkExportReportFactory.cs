using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting
{
    public class NetworkExportReportFactory : IReportFactory
    {
        public string Name => "NetworkExportReport";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            var report = new NetworkExportReport(uow, Name, results, hubService);
            return report;
        }
    }
}
