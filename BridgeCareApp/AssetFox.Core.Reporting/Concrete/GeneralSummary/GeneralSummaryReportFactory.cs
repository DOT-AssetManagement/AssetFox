using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Interfaces;

namespace AssetFox.Core.Reporting.Concrete.GeneralSummary
{
    public class GeneralSummaryReportFactory : IReportFactory
    {
        public string Name => "GeneralSummaryReport";

        public IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "")
        {
            var report = new GeneralSummaryReport(uow, Name, results, hubService);
            return report;
        }
    }
}
