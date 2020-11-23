using BridgeCare.Models;

namespace BridgeCare.Interfaces.SummaryReport
{
    public interface ISummaryReportGenerator
    {
        void GenerateExcelReport(SimulationModel simulationModel);
        byte[] DownloadExcelReport(SimulationModel simulationModel);
    }
}
