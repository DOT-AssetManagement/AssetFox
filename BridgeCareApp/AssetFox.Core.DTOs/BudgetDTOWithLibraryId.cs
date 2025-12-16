using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DTOs
{
    public class BudgetDTOWithLibraryId
    {
        public BudgetDTO Budget { get; set; }
        public Guid BudgetLibraryId { get; set; }
    }
}
