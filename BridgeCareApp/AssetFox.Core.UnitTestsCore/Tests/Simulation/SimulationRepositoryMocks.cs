using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using Moq;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationRepositoryMocks
    {
        public static Mock<ISimulationRepository> New(Mock<IUnitOfWork> unitOfWork = null)
        {
            var repository = new Mock<ISimulationRepository>();
            if (unitOfWork != null)
            {
                unitOfWork.Setup(u => u.SimulationRepo).Returns(repository.Object);
            }
            return repository;
        }

        public static void SetupGetSimulation(this Mock<ISimulationRepository> mock, SimulationDTO simulation)
        {
            mock.Setup(s => s.GetSimulation(simulation.Id)).Returns(simulation);
        }
    }
}
