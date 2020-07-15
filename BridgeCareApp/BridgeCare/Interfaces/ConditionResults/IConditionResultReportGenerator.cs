using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeCare.Models;

namespace BridgeCare.Interfaces.ConditionResults
{
    public interface IConditionResultReportGenerator
    {
        void GenerateConditionResultReport(SimulationModel simulationModel);
    }
}
