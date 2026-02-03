using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using Moq;

namespace AssetFoxCoreTests.Tests.BudgetPriority
{
    public static class BudgetPriorityRepositoryMocks
    {
        public static Mock<IBudgetPriorityRepository> New(Mock<IUnitOfWork> mockUnitOfWork = null)
        {
            var mockBudgetPriorityRepository = new Mock<IBudgetPriorityRepository>();
            if (mockUnitOfWork != null)
            {
                mockUnitOfWork.Setup(u => u.BudgetPriorityRepo).Returns(mockBudgetPriorityRepository.Object);
            }
            return mockBudgetPriorityRepository;
        }
        public static void SetupGetLibraryAccess(this Mock<IBudgetPriorityRepository> mock, Guid libraryId, LibraryUserAccessModel accessModel)
        {
            mock.Setup(r => r.GetLibraryAccess(libraryId, It.IsAny<Guid>())).Returns(accessModel);
        }
    }
}
