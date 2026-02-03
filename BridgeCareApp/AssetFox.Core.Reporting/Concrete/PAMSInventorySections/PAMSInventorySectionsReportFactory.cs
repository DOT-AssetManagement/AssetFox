using System;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting
{
    public class PAMSInventorySectionsReportFactory : IReportFactory
    {
        public string Name => "PAMSInventoryLookupSections";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            return new PAMSInventorySectionsReport(uow, Name, results, suffix);
        }
    }
}
