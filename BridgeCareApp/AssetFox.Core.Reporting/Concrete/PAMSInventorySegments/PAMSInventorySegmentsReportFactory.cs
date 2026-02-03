using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting
{
    public class PAMSInventorySegmentsReportFactory: IReportFactory
    {
        public string Name => "PAMSInventoryLookupSegments";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            return new PAMSInventorySegmentsReport(uow, Name, results, suffix);
        }
    }
}
