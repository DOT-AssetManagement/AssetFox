using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IReportIndexRepository
    {
        bool Add(ReportIndexDTO report);
        ReportIndexDTO Get(Guid reportId);
        List<ReportIndexDTO> GetAllForScenario(Guid scenarioId);
        bool DeleteReport(Guid reportId);
        bool DeleteAllSimulationReports(Guid scenarioId);
        /// <summary>
        /// Deletes any ReportIndex with an ExpirationDate before the current date
        /// </summary>
        bool DeleteExpiredReports();
        IList<Generics.ReportItemList> GetAllReportsInSystem();
    }
}
