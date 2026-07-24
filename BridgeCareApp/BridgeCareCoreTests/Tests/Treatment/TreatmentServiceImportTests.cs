using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services;
using AssetFoxCore.Services.Treatment;
using Moq;
using OfficeOpenXml;
using Xunit;

namespace AssetFoxCoreTests.Tests.Treatment
{
    public class TreatmentServiceImportTests
    {
        [Fact]
        public void ImportLibraryTreatmentsFileSingle_MultipleUniqueNames_AddsAllTreatments()
        {
            var libraryId = Guid.NewGuid();
            var unitOfWork = new Mock<IUnitOfWork>();
            var treatmentRepository = new Mock<ISelectableTreatmentRepository>();
            unitOfWork.SetupGet(work => work.SelectableTreatmentRepo).Returns(treatmentRepository.Object);
            treatmentRepository
                .Setup(repository => repository.GetSelectableTreatmentNames(libraryId))
                .Returns(new List<string>());
            treatmentRepository
                .Setup(repository => repository.GetSingleTreatmentLibary(libraryId))
                .Returns(new TreatmentLibraryDTO
                {
                    Id = libraryId,
                    Name = "Library",
                    Description = string.Empty
                });

            using var excelPackage = new ExcelPackage();
            TreatmentWorksheetGenerator.Fill(excelPackage.Workbook, new List<TreatmentDTO>
            {
                CreateTreatment("First Treatment"),
                CreateTreatment("Second Treatment")
            });

            var service = new TreatmentService(unitOfWork.Object, new ExcelTreatmentLoader(Mock.Of<IExpressionValidationService>()));

            service.ImportLibraryTreatmentsFileSingle(libraryId, excelPackage);

            treatmentRepository.Verify(repository => repository.AddLibraryTreatments(
                It.Is<List<TreatmentDTO>>(treatments =>
                    treatments.Count == 2 &&
                    treatments.Any(treatment => treatment.Name == "First Treatment") &&
                    treatments.Any(treatment => treatment.Name == "Second Treatment")),
                libraryId));
        }

        [Fact]
        public void ImportLibraryTreatmentsFileSingle_DuplicateExistingName_RejectsEntireImport()
        {
            var libraryId = Guid.NewGuid();
            var unitOfWork = new Mock<IUnitOfWork>();
            var treatmentRepository = new Mock<ISelectableTreatmentRepository>();
            unitOfWork.SetupGet(work => work.SelectableTreatmentRepo).Returns(treatmentRepository.Object);
            treatmentRepository
                .Setup(repository => repository.GetSelectableTreatmentNames(libraryId))
                .Returns(new List<string> { "Existing Treatment" });

            using var excelPackage = new ExcelPackage();
            excelPackage.Workbook.Worksheets.Add("existing treatment");
            excelPackage.Workbook.Worksheets.Add("New Treatment");

            var service = new TreatmentService(unitOfWork.Object, new ExcelTreatmentLoader(Mock.Of<IExpressionValidationService>()));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.ImportLibraryTreatmentsFileSingle(libraryId, excelPackage));

            Assert.Contains("Existing Treatment", exception.Message, StringComparison.OrdinalIgnoreCase);
            treatmentRepository.Verify(
                repository => repository.AddLibraryTreatments(It.IsAny<List<TreatmentDTO>>(), libraryId),
                Times.Never);
        }

        [Fact]
        public void ImportScenarioTreatmentsFileSingle_DuplicateExistingName_RejectsEntireImport()
        {
            var simulationId = Guid.NewGuid();
            var unitOfWork = new Mock<IUnitOfWork>();
            var treatmentRepository = new Mock<ISelectableTreatmentRepository>();
            unitOfWork.SetupGet(work => work.SelectableTreatmentRepo).Returns(treatmentRepository.Object);
            treatmentRepository
                .Setup(repository => repository.GetScenarioSelectableTreatmentNames(simulationId))
                .Returns(new List<string> { "Existing Treatment" });

            using var excelPackage = new ExcelPackage();
            excelPackage.Workbook.Worksheets.Add("Existing Treatment");
            excelPackage.Workbook.Worksheets.Add("New Treatment");

            var service = new TreatmentService(unitOfWork.Object, new ExcelTreatmentLoader(Mock.Of<IExpressionValidationService>()));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.ImportScenarioTreatmentsFileSingle(simulationId, excelPackage));

            Assert.Contains("Existing Treatment", exception.Message, StringComparison.OrdinalIgnoreCase);
            treatmentRepository.Verify(
                repository => repository.AddScenarioSelectableTreatment(It.IsAny<List<TreatmentDTO>>(), simulationId),
                Times.Never);
        }

        private static TreatmentDTO CreateTreatment(string name)
        {
            return new TreatmentDTO
            {
                Name = name,
                Description = string.Empty,
                AssetType = string.Empty,
                CriterionLibrary = new CriterionLibraryDTO
                {
                    MergedCriteriaExpression = string.Empty
                },
                Costs = new List<TreatmentCostDTO>(),
                Consequences = new List<TreatmentConsequenceDTO>(),
                PerformanceFactors = new List<TreatmentPerformanceFactorDTO>(),
                Budgets = new List<TreatmentBudgetDTO>()
            };
        }
    }
}
