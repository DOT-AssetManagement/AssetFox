using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.Validation;
using Moq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public class AnalysisInputLoadingTests
    {
        [Fact]
        public void GetSimulationWithoutAssets_EverythingThrows_ValidationResultsAppearInBag()
        {
            var networkId = Guid.NewGuid();
            var simulationId = Guid.NewGuid();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var attributeRepositoryMock = AttributeRepositoryMocks.New(unitOfWorkMock);
            attributeRepositoryMock.Setup(a => a.GetExplorer()).Throws(new Exception("GetExplorer failed"));
            var networkRepositoryMock = NetworkRepositoryMocks.New(unitOfWorkMock);
            networkRepositoryMock.Setup(n => n.GetSimulationAnalysisNetwork(networkId, null, false, simulationId))
                .Throws(new Exception("GetSimulationAnalysisNetwork failed"));
            var simulationRepository = SimulationRepositoryMocks.DefaultMock(unitOfWorkMock);
            simulationRepository.Setup(s => s.GetSimulationInNetwork(simulationId, null)).Throws(new Exception("GetSimulationInNetwork failed"));
            var validationResultBag = new ValidationResultBag();
            AnalysisInputLoading.GetSimulationWithoutAssets(
                unitOfWorkMock.Object,
                networkId,
                simulationId,
                validationResultBag);
            var resultList = new List<ValidationResult>();
            foreach (var result in validationResultBag)
            {
                resultList.Add(result);
            }
            var dummyNullReferenceException = new NullReferenceException();
            Assert.Single(resultList, r => r.Message == "GetExplorer failed");
            Assert.Single(resultList, r => r.Message == "GetSimulationAnalysisNetwork failed");
            Assert.Single(resultList, r => r.Message == "GetSimulationInNetwork failed");
            Assert.Single(resultList, r => r.Message == dummyNullReferenceException.Message);
        }
    }
}
