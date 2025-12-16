using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.CashFlowRule
{
    public static class CashFlowRuleLibraryUserTestSetup
    {
        public static void SetUsersOfCashFlowRuleLibrary(
            IUnitOfWork unitOfWork,
            Guid cashFlowRuleLibraryId,
            LibraryAccessLevel accessLevelForAllListedUsers,
            params Guid[] userIds)
        {
            var dtos = new List<LibraryUserDTO>();
            foreach (var userId in userIds)
            {
                var libraryUserDto = new LibraryUserDTO
                {
                    AccessLevel = accessLevelForAllListedUsers,
                    UserId = userId,
                };
                dtos.Add(libraryUserDto);
            }
            unitOfWork.CashFlowRuleRepo.UpsertOrDeleteUsers(cashFlowRuleLibraryId, dtos);
        }
    }
}
