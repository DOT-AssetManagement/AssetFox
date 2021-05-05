using BridgeCare.Models.SummaryReport;

namespace BridgeCare.Interfaces.SummaryReport
{
    public interface ICommonSummaryReportData
    {
        SimulationYearsModel GetSimulationYearsData(int simulationId);
    }
}
