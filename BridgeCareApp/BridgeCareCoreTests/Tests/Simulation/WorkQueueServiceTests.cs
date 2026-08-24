using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.WorkQueue;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests;

public class WorkQueueServiceTests
{
    [Fact]
    public void GetWorkQueuePage_UsesRepositoryFromItsOwnRequestScope()
    {
        var firstRepository = CreateRepository();
        var secondRepository = CreateRepository();
        var unitOfWork = new Mock<IUnitOfWork>();
        var queue = new SequentialWorkQueue<WorkQueueMetadata>();
        var fastQueue = new FastSequentialworkQueue<WorkQueueMetadata>();
        var hiddenQueue = new HiddenUploadQueue<WorkQueueMetadata>();
        var firstService = new WorkQueueService(
            unitOfWork.Object,
            queue,
            fastQueue,
            hiddenQueue,
            firstRepository.Object);

        _ = new WorkQueueService(
            unitOfWork.Object,
            queue,
            fastQueue,
            hiddenQueue,
            secondRepository.Object);

        firstService.GetWorkQueuePage(new PagingRequestModel<QueuedWorkDTO>());

        firstRepository.Verify(
            repository => repository.GetScenariosWithIds(It.IsAny<List<Guid>>()),
            Times.Once);
        secondRepository.Verify(
            repository => repository.GetScenariosWithIds(It.IsAny<List<Guid>>()),
            Times.Never);
    }

    private static Mock<ISimulationRepository> CreateRepository()
    {
        var repository = new Mock<ISimulationRepository>();
        repository
            .Setup(item => item.GetScenariosWithIds(It.IsAny<List<Guid>>()))
            .Returns(new List<SimulationDTO>());
        return repository;
    }
}
