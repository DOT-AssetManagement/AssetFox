using System.Collections.Generic;
using BridgeCare.Models;
using BridgeCare.Models.Inventory;

namespace BridgeCare.Interfaces
{
    public interface IInventory
    {
        InventoryModel GetInventoryByBMSId(string bmsId, BridgeCareContext db);

        InventoryModel GetInventoryByBRKey(string brKey, BridgeCareContext db);

        List<InventorySelectionModel> GetInventorySelectionModels(BridgeCareContext db, UserInformationModel userInformation);
    }
}
