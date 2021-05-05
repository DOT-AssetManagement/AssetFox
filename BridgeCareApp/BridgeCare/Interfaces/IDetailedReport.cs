using System.Collections.Generic;
using System.Linq;
using BridgeCare.DataAccessLayer;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface IDetailedReport
    {
        List<YearlyDataModel> GetYearsData(SimulationModel data);

        IQueryable<DetailedReportDAL> GetRawQuery(SimulationModel data, BridgeCareContext db);
    }
}
