using System.Collections.Generic;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface INetwork
    {
        List<NetworkModel> GetAllNetworks(BridgeCareContext db);
    }
}
