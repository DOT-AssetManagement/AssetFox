using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeCare.Models;

namespace BridgeCare.Interfaces.ReportsDownload
{
    // Added type T because unity container needs class name with interface name to register it.
    // Also, at the time of Injection T tells unity container with instance to inject
    public interface IReportsDownload<T> where T: class
    {
        byte[] DownloadExcelReport(SimulationModel simulationModel);
    }
}
