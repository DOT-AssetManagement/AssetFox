using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Moq;

namespace AssetFoxCoreTests.Tests.Announcement
{
    public static class AnnouncementRepositoryMocks
    {
        public static Mock<IAnnouncementRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var mock = new Mock<IAnnouncementRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.AnnouncementRepo).Returns(mock.Object);
            }
            return mock;
        }
    }
}
