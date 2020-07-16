using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BridgeCare.Models;

namespace BridgeCare.Interfaces.BudgetResults
{
    public interface IBudgetResultReportGenerator
    {
        void GenerateBudgetResultReport(SimulationModel simulationModel);
    }
}
