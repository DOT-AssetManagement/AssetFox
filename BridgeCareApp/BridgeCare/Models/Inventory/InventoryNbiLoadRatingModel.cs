using System.Collections.Generic;

namespace BridgeCare.Models.Inventory
{
    public class InventoryNbiLoadRatingModel
    {
        public InventoryNbiLoadRatingModel()
        {
            NbiLoadRatingItems = new List<InventoryItemModel>();
        }

        public List<InventoryItemModel> NbiLoadRatingItems { get; set; }
    }
}
