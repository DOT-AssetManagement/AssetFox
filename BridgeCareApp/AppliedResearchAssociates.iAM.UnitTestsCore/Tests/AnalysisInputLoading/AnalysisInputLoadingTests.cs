using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using AppliedResearchAssociates.Validation;
using BridgeCareCoreTests.Tests;
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
            var userId = Guid.NewGuid();
            var user = new UserDTO
            {
                Id = userId,
                Username = "Username",
            };
            var unitOfWork = UnitOfWorkMocks.WithCurrentUser(user);

            var attributeRepositoryMock = AttributeRepositoryMocks.New(unitOfWork);
            attributeRepositoryMock.Setup(a => a.GetExplorer()).Throws(new Exception("GetExplorer failed"));
            var attributeNameLookupDictionary = new Dictionary<Guid, string>();
            attributeRepositoryMock.Setup(a => a.GetAttributes()).Returns(new List<AttributeDTO>());
            var networkRepositoryMock = NetworkRepositoryMocks.New(unitOfWork);
            networkRepositoryMock.Setup(n => n.GetSimulationAnalysisNetwork(networkId, null, false, simulationId))
                .Throws(new Exception("GetSimulationAnalysisNetwork failed"));
            var simulationRepository = SimulationRepositoryMocks.New(unitOfWork);
            simulationRepository.Setup(s => s.GetSimulationInNetwork(simulationId, null)).Throws(new Exception("GetSimulationInNetwork failed"));
            var userCriteriaRepository = UserCriteriaRepositoryMocks.New(unitOfWork);
            userCriteriaRepository.Setup(u => u.GetUserCriteria(userId)).Returns("userCriteria");
            var analysisMethodRepository = AnalysisMethodRepositoryMocks.New(unitOfWork);
            analysisMethodRepository.Setup(a => a.GetSimulationAnalysisMethod(null, "userCriteria")).Throws(new Exception("GetSimulationAnalysisMethod failed"));
            var performanceCurveRepository = PerformanceCurveRepositoryMocks.New(unitOfWork);
            performanceCurveRepository.Setup(p => p.GetScenarioPerformanceCurves(null, It.IsAny<Dictionary<Guid, string>>()))
                .Throws(new Exception("GetScenarioPerformanceCurves failed"));
            var selectableTreatmentRepository = SelectableTreatmentRepositoryMocks.New(unitOfWork);
            selectableTreatmentRepository.Setup(s => s.GetScenarioSelectableTreatments(null)).Throws(new Exception("GetScenarioSelectableTreatments failed"));
            var committedProjectRepository = CommittedProjectRepositoryMocks.New(unitOfWork);
            committedProjectRepository.Setup(c => c.GetSimulationCommittedProjects(null)).Throws(new Exception("GetSimulationCommittedProjects failed"));
            var calculatedAttributeRepository = CalculatedAttributeRepositoryMocks.New(unitOfWork);
            calculatedAttributeRepository.Setup(c => c.PopulateScenarioCalculatedFields(null)).Throws(new Exception("PopulateScenarioCalculatedFields failed"));
            var validationResultBag = new ValidationResultBag();
            AnalysisInputLoading.GetSimulationWithoutAssets(
                unitOfWork.Object,
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
            Assert.Contains(resultList, r => r.Message == dummyNullReferenceException.Message);
            Assert.Single(resultList, r => r.Message == "GetSimulationAnalysisMethod failed");
            Assert.Single(resultList, r => r.Message == "GetScenarioPerformanceCurves failed");
            Assert.Single(resultList, r => r.Message == "GetScenarioSelectableTreatments failed");
            Assert.Single(resultList, r => r.Message == "GetSimulationCommittedProjects failed");
            Assert.Single(resultList, r => r.Message == "PopulateScenarioCalculatedFields failed");
        }
    }
}
