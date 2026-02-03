using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class BudgetAmountDTOWithBudgetId
    {
        public BudgetAmountDTO BudgetAmount { get; set; }
        public Guid BudgetId { get; set; }
    }
}
