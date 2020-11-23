using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface ICashFlowLibrary
    {
        CashFlowLibraryModel GetSimulationCashFlowLibrary(int id, BridgeCareContext db);

        CashFlowLibraryModel SaveSimulationCashFlowLibrary(CashFlowLibraryModel model, BridgeCareContext db);
    }
}
