using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting
{
    public class RawDataNetworkExportReportFactory : IReportFactory
    {
        public string Name => "RawDataNetworkExportReport";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            var report = new RawDataNetworkExportReport(uow, Name, results, hubService);
            return report;
        }
    }
}
