using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using Moq;

namespace AssetFoxCoreTests.Tests.BudgetPriority
{
    public static class BudgetPriorityPagingServiceMocks
    {
        public static Mock<IBudgetPriortyPagingService> New()
            => new Mock<IBudgetPriortyPagingService>();
        public static void SetupGetSyncedLibraryDataset(this Mock<IBudgetPriortyPagingService> mock, LibraryUpsertPagingRequestModel<BudgetPriorityLibraryDTO, BudgetPriorityDTO> upsertRequest)
        {
            mock.Setup(r => r.GetSyncedLibraryDataset(upsertRequest)).Returns(new List<BudgetPriorityDTO>());
        }
    }
}
