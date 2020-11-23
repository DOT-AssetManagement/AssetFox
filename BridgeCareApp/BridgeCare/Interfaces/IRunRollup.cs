using System.Threading.Tasks;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface IRunRollup
    {
        void SetLastRunDate(int networkId, BridgeCareContext db);

        Task<string> RunRollup(SimulationModel model);
    }
}
