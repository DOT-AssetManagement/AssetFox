using System.Linq;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Benefit;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public class AnalysisMethodRepositoryTests
    {
        [Fact]
        public void GetAnalysisMethod_AnalysisMethodInDb_Gets()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var entity = AnalysisMethodEntities.TestAnalysis(simulation.Id);
            TestHelper.UnitOfWork.Context.AnalysisMethod.Add(entity);
            TestHelper.UnitOfWork.Context.SaveChanges();
            // Act
            var result = unitOfWork.AnalysisMethodRepo.GetAnalysisMethod(simulation.Id);
            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public void UpsertAnalysisMethod_AnalysisMethodAlreadyInDb_UpdatesBenefit()
        {
            // Arrange
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;
            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            analysisMethodDto.Benefit = BenefitDtos.Dto(TestAttributeNames.Age);

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            // Assert
            var upsertedAnalysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            Assert.Equal(analysisMethodDto.Id, upsertedAnalysisMethodDto.Id);
            Assert.Equal(analysisMethodDto.Benefit.Id, upsertedAnalysisMethodDto.Benefit.Id);
        }

        [Fact]
        public void GetAnalysisMethodSetting_AnalysisMethodDoesNotExist_False()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;

            var analysisMethodSetting = unitOfWork.AnalysisMethodRepo.GetSimulationAnalysisMethodSetting(simulation.Id);

            Assert.False(analysisMethodSetting);
        }

        [Fact]
        public void GetAnalysisMethodSetting_AnalysisMethodExists_True()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;
            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            analysisMethodDto.Benefit = BenefitDtos.Dto(TestAttributeNames.Age);

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            var analysisMethodSetting = unitOfWork.AnalysisMethodRepo.GetSimulationAnalysisMethodSetting(simulation.Id);

            Assert.True(analysisMethodSetting);

        }

        [Fact]
        public void UpsertAnalysisMethod_AnalysisMethodInDb_Updates()
        {
            var unitOfWork = TestHelper.UnitOfWork;
            AttributeTestSetup.CreateAttributes(unitOfWork);
            NetworkTestSetup.CreateNetwork(unitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var criterionLibrary = CriterionLibraryTestSetup.TestCriterionLibraryInDb(TestHelper.UnitOfWork);
            var repo = unitOfWork.AnalysisMethodRepo;

            var analysisMethodDto = repo.GetAnalysisMethod(simulation.Id);
            var attributeEntity = TestHelper.UnitOfWork.Context.Attribute.First();
            analysisMethodDto.Attribute = attributeEntity.Name;
            analysisMethodDto.CriterionLibrary = criterionLibrary;
            var analysisMethod = AnalysisMethodEntities.TestAnalysis(simulation.Id);
            var benefitDto = BenefitDtos.Dto(attributeEntity.Name);
            analysisMethodDto.Benefit = benefitDto;

            // Act
            repo.UpsertAnalysisMethod(simulation.Id, analysisMethodDto);

            // Assert
            var analysisMethodDtoAfter = repo.GetAnalysisMethod(simulation.Id);

            Assert.Equal(analysisMethodDtoAfter.Id, analysisMethodDto.Id);
            Assert.Equal(analysisMethodDtoAfter.Attribute, analysisMethodDto.Attribute);
            Assert.Equal(analysisMethodDtoAfter.CriterionLibrary.Id, analysisMethodDto.CriterionLibrary.Id);
            Assert.Equal(analysisMethodDtoAfter.Benefit.Id, analysisMethodDto.Benefit.Id);
            Assert.Equal(analysisMethodDtoAfter.Benefit.Attribute, analysisMethodDto.Benefit.Attribute);
        }
    }
}
