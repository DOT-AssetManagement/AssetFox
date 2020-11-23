using BridgeCare.Models.Inventory;

namespace BridgeCare.Interfaces
{
    public interface IInventoryItemDetailModelGenerator
    {
        InventoryItemDetailModel MakeInventoryItemDetailModel(InventoryModel inventoryModel);
    }
}
