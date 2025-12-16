using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class BudgetPriorityLibraryDtos
    {

        public const string BudgetPriorityLibraryName = "BudgetPriorityLibrary";

        public static BudgetPriorityLibraryDTO New(Guid? id = null)
        {
            var dto = new BudgetPriorityLibraryDTO
            {
                Id = id ?? Guid.NewGuid(),
                Name = BudgetPriorityLibraryName,
                BudgetPriorities = new List<BudgetPriorityDTO>(),
            };
            return dto;
        }
    }
}
