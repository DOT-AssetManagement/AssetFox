using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting
{
    public class BAMSInventoryReportFactory : IReportFactory
    {
        public string Name => "BAMSInventoryLookup";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            return new BAMSInventoryReport(uow, Name, results, suffix);
        }
    }
}
