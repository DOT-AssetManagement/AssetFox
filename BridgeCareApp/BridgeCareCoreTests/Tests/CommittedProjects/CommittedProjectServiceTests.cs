using Moq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs.Abstract;
using System.Data;
using OfficeOpenXml;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFoxCore.Services.CommittedProjects;

namespace AssetFoxCoreTests.Tests
{
    public class CommittedProjectServiceTests : IClassFixture<ExcelAccess>
    {
        private IUnitOfWork _testUOW;
        private Mock<ISimulationRepository> _mockedSimulationRepo;
        private Mock<ICommittedProjectRepository> _mockCommittedProjectRepo;
        private Mock<INetworkRepository> _mockNetworkRepo;
        private Mock<ISelectableTreatmentRepository> _mockSelectableTreatmentRepo;
        private Guid _badScenario = Guid.Parse("0c66674c-8fcb-462b-8765-69d6815e0958");

        private ExcelPackage _excelData; // passed in via the constructor on ExcelAccess.

        public CommittedProjectServiceTests(ExcelAccess excelData)
        {
            _excelData = excelData.ExcelData;

            var mockedTestUOW = new Mock<IUnitOfWork>();
            var mockInvestmentPlanRepo = new Mock<IInvestmentPlanRepository>();
            mockedTestUOW.Setup(u => u.InvestmentPlanRepo).Returns(mockInvestmentPlanRepo.Object);
            mockInvestmentPlanRepo.Setup(i => i.GetInvestmentPlan(TestDataForCommittedProjects.SimulationId)).Returns(TestDataForCommittedProjects.TestInvestmentPlan);
            int[] years = { 2022, 2023 };
            mockInvestmentPlanRepo.Setup(i => i.GetInvestmentStartAndEndYears(It.IsAny<Guid>())).Returns(years);
            var mockAssetDataRepository = new Mock<IAssetData>();
            mockAssetDataRepository.Setup(_ => _.KeyProperties).Returns(TestDataForCommittedProjects.KeyProperties);
            mockedTestUOW.Setup(_ => _.AssetDataRepository).Returns(mockAssetDataRepository.Object);
            _mockSelectableTreatmentRepo = new Mock<ISelectableTreatmentRepository>();
            mockedTestUOW.Setup(_ => _.SelectableTreatmentRepo).Returns(_mockSelectableTreatmentRepo.Object);
            _mockCommittedProjectRepo = new Mock<ICommittedProjectRepository>();
            _mockCommittedProjectRepo.Setup(_ => _.GetCommittedProjectsForExport(It.IsAny<Guid>()))
                .Returns<Guid>(_ => TestDataForCommittedProjects.ValidCommittedProjects
                    .Where(q => q.SimulationId == _)
                    .Select(p => (BaseCommittedProjectDTO)p)
                    .ToList());
            mockedTestUOW.Setup(_ => _.CommittedProjectRepo).Returns(_mockCommittedProjectRepo.Object);

            _mockedSimulationRepo = new Mock<ISimulationRepository>();
            _mockedSimulationRepo.Setup(s => s.GetSimulation(TestDataForCommittedProjects.SimulationId)).Returns(TestDataForCommittedProjects.TestSimulationDTO);
            _mockedSimulationRepo.Setup(s => s.GetSimulation(TestDataForCommittedProjects.NoCommitSimulationId)).Returns(TestDataForCommittedProjects.TestSimulationDTONoCommittedProjects);
            _mockedSimulationRepo.Setup(_ => _.GetSimulationName(It.Is<Guid>(_ => _ != _badScenario))).Returns("Test");
            _mockedSimulationRepo.Setup(_ => _.GetSimulationName(It.Is<Guid>(_ => _ == _badScenario))).Returns<string>(null);
            _mockedSimulationRepo.Setup(_ => _.GetSimulation(It.Is<Guid>(_ => _ == _badScenario))).Throws<RowNotInTableException>();
            mockedTestUOW.Setup(_ => _.SimulationRepo).Returns(_mockedSimulationRepo.Object);

            _mockNetworkRepo = new Mock<INetworkRepository>();
            _mockNetworkRepo.Setup(n => n.GetNetworkKeyAttribute(TestDataForCommittedProjects.NetworkId)).Returns(TestAttributeNames.BrKey);
            mockedTestUOW.Setup(_ => _.NetworkRepo).Returns(_mockNetworkRepo.Object);

            var mockAttributeRepository = new Mock<IAttributeRepository>();
            mockAttributeRepository.Setup(_ => _.GetAttributes()).Returns(TestDataForCommittedProjects.Attributes);
            mockedTestUOW.Setup(_ => _.AttributeRepo).Returns(mockAttributeRepository.Object);

            var mockMaintainableAssetRepository = new Mock<IMaintainableAssetRepository>();
            mockMaintainableAssetRepository.Setup(_ => _.GetAllInNetworkWithLocations(It.IsAny<Guid>()))
                .Returns(TestDataForCommittedProjects.MaintainableAssets);
            mockedTestUOW.Setup(_ => _.MaintainableAssetRepo).Returns(mockMaintainableAssetRepository.Object);

            var mockBudgetRepository = new Mock<IBudgetRepository>();
            mockBudgetRepository.Setup(_ => _.GetScenarioBudgets(It.IsAny<Guid>())).Returns(TestDataForCommittedProjects.ScenarioBudgets);
            mockedTestUOW.Setup(_ => _.BudgetRepo).Returns(mockBudgetRepository.Object);

            var adminSettingsRepo = AdminSettingsRepositoryMocks.New(mockedTestUOW);
            adminSettingsRepo.Setup(a => a.GetKeyFields()).Returns(new List<string>{TestAttributeNames.BrKey, TestAttributeNames.BmsId});

            //_testUOW = new UnitOfDataPersistenceWork(new Mock<IConfiguration>().Object, _mockedContext.Object);
            _testUOW = mockedTestUOW.Object;
        }

        [Fact]
        public void ExportValidWithGoodData()
        {
            // Arrange
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);

            // Act
            var result = service.ExportCommittedProjectsFile(TestDataForCommittedProjects.SimulationId);

            // Asset
            Assert.False(string.IsNullOrEmpty(result.FileName));
            Assert.True(result.FileData.Length > 0);
            var excel = new ExcelPackage(new System.IO.MemoryStream(Convert.FromBase64String(result.FileData)));
            Assert.True(excel.Workbook.Worksheets.Count > 0);
            var cells = excel.Workbook.Worksheets[0].Cells.Value;
            Assert.NotNull(cells);
            var cellArray = (Array)cells;
            var cellArrayLength = cellArray.GetLength(0);
            var committedProjectsForThisSimulation = TestDataForCommittedProjects.ValidCommittedProjects.Where(cp => cp.SimulationId == TestDataForCommittedProjects.SimulationId).ToList();
            Assert.Equal(committedProjectsForThisSimulation.Count + 1, cellArrayLength);
        }

        [Fact]
        public void ExportHandlesInvalidScenarioId()
        {
            // Arrange
            _mockCommittedProjectRepo.Setup(_ => _.GetCommittedProjectsForExport(It.IsAny<Guid>())).Throws<RowNotInTableException>();
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => service.ExportCommittedProjectsFile(_badScenario));
        }

        [Fact]
        public void ExportProvidesATemplateWhenNoCommittedProjectsExist()
        {
            // Arrange
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);

            // Act
            var result = service.ExportCommittedProjectsFile(TestDataForCommittedProjects.NoCommitSimulationId);

            // Assert
            Assert.False(string.IsNullOrEmpty(result.FileName));
            Assert.True(result.FileData.Length > 0);
            var excel = new ExcelPackage(new System.IO.MemoryStream(Convert.FromBase64String(result.FileData)));
            Assert.True(excel.Workbook.Worksheets.Count > 0);
            var cells = excel.Workbook.Worksheets[0].Cells.Value;
            Assert.NotNull(cells);
            Assert.Equal(1, ((Array)cells).GetLength(0));
        }

        [Fact]
        public void ServiceInstancesKeepTheirOwnScopedUnitOfWork()
        {
            // Arrange
            var firstUnitOfWork = new Mock<IUnitOfWork>();
            var firstMaintainableAssetRepository = new Mock<IMaintainableAssetRepository>();
            firstUnitOfWork.Setup(_ => _.MaintainableAssetRepo).Returns(firstMaintainableAssetRepository.Object);

            var secondUnitOfWork = new Mock<IUnitOfWork>();
            var secondMaintainableAssetRepository = new Mock<IMaintainableAssetRepository>();
            secondUnitOfWork.Setup(_ => _.MaintainableAssetRepo).Returns(secondMaintainableAssetRepository.Object);

            var hubService = HubServiceMocks.Default();
            var firstService = new CommittedProjectService(firstUnitOfWork.Object, hubService);
            _ = new CommittedProjectService(secondUnitOfWork.Object, hubService);

            // Act
            firstService.GetTreatmentCost("asset-key", Guid.NewGuid(), Guid.NewGuid());

            // Assert
            firstMaintainableAssetRepository.Verify(
                _ => _.GetMaintainableAssetByKeyAttribute(It.IsAny<Guid>(), "asset-key"),
                Times.Once);
            secondMaintainableAssetRepository.Verify(
                _ => _.GetMaintainableAssetByKeyAttribute(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ImportHandlesBadSimulationId()
        {
            // Arrange
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);
            var userId = "Ignored user id";

            // Act & Assert
            Assert.Throws<RowNotInTableException>(() => service.ImportCommittedProjectFiles(_badScenario, new ExcelPackage(), "Bad File", userId));
        }

        [Fact]
        public void ImportCreatesValidRecordsWithoutNoTreatment()
        {
            // Arrange
            var userId = "ignored user id";
            List<SectionCommittedProjectDTO> testInput = new List<SectionCommittedProjectDTO>();
            _mockCommittedProjectRepo.Setup(_ => _.UpsertCommittedProjects(It.IsAny<List<SectionCommittedProjectDTO>>()))
                .Callback<List<SectionCommittedProjectDTO>>(_ =>
                {
                    testInput = _;
                });
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);
            const string networkKeyAttribute = TestAttributeNames.BrKey;
            var treatmentNames = new List<string>
            {
                "Rehabilitation",
                "Maintenance"
            };
            _mockSelectableTreatmentRepo.Setup(m => m.GetScenarioSelectableTreatmentNames(TestDataForCommittedProjects.SimulationId))
                .Returns(treatmentNames);

            service.ImportCommittedProjectFiles(TestDataForCommittedProjects.SimulationId, _excelData, "GoodFile", userId);

            // Assert
            Assert.True(testInput.Count == 2, "Number of comitted projects is wrong");
            Assert.True(testInput[0] is SectionCommittedProjectDTO, "Provided value is not a Section type");
            Assert.True(testInput[0].VerifyLocation(networkKeyAttribute), "Could not verify location");
            Assert.Equal(2023, testInput[1].Year);
        }

        [Fact]
        public void ImportPrefersCommittedProjectsWorksheetWhenItIsNotFirst()
        {
            // Arrange
            var importedProjects = new List<SectionCommittedProjectDTO>();
            _mockCommittedProjectRepo
                .Setup(repository => repository.UpsertCommittedProjects(It.IsAny<List<SectionCommittedProjectDTO>>()))
                .Callback<List<SectionCommittedProjectDTO>>(projects => importedProjects = projects);
            _mockSelectableTreatmentRepo
                .Setup(repository => repository.GetScenarioSelectableTreatmentNames(TestDataForCommittedProjects.SimulationId))
                .Returns(["Rehabilitation", "Maintenance"]);

            using var workbook = new ExcelPackage();
            var instructions = workbook.Workbook.Worksheets.Add("Instructions");
            instructions.Cells[1, 1].Value = "This sheet is intentionally first.";
            workbook.Workbook.Worksheets.Add("Committed Projects", _excelData.Workbook.Worksheets[0]);

            var service = new CommittedProjectService(_testUOW, HubServiceMocks.Default());

            // Act
            service.ImportCommittedProjectFiles(
                TestDataForCommittedProjects.SimulationId,
                workbook,
                "ErrorsWorkbook.xlsx",
                "ignored user id");

            // Assert
            Assert.Equal(2, importedProjects.Count);
        }

        [Fact]
        public void ImportFallsBackToFirstWorksheetWhenCommittedProjectsWorksheetIsAbsent()
        {
            // Arrange
            var importedProjects = new List<SectionCommittedProjectDTO>();
            _mockCommittedProjectRepo
                .Setup(repository => repository.UpsertCommittedProjects(It.IsAny<List<SectionCommittedProjectDTO>>()))
                .Callback<List<SectionCommittedProjectDTO>>(projects => importedProjects = projects);
            _mockSelectableTreatmentRepo
                .Setup(repository => repository.GetScenarioSelectableTreatmentNames(TestDataForCommittedProjects.SimulationId))
                .Returns(["Rehabilitation", "Maintenance"]);

            using var workbook = new ExcelPackage();
            workbook.Workbook.Worksheets.Add("Client Upload", _excelData.Workbook.Worksheets[0]);
            var service = new CommittedProjectService(_testUOW, HubServiceMocks.Default());

            // Act
            service.ImportCommittedProjectFiles(
                TestDataForCommittedProjects.SimulationId,
                workbook,
                "InitialUpload.xlsx",
                "ignored user id");

            // Assert
            Assert.Equal(2, importedProjects.Count);
        }

        [Fact]
        public void ImportErrorWorkbookExplainsAndLinksToInvalidCells()
        {
            // Arrange
            _mockSelectableTreatmentRepo
                .Setup(repository => repository.GetScenarioSelectableTreatmentNames(It.IsAny<Guid>()))
                .Returns(["Rehabilitation", "Maintenance"]);

            var simulation = TestDataForCommittedProjects.TestSimulationDTO();
            simulation.Id = Guid.NewGuid();
            var outputFolder = Path.Combine("CommittedProjects", simulation.Id.ToString());

            using var upload = new ExcelPackage();
            var uploadWorksheet = upload.Workbook.Worksheets.Add("Client Upload", _excelData.Workbook.Worksheets[0]);
            var projectSourceColumn = Enumerable.Range(1, uploadWorksheet.Dimension.End.Column)
                .Single(column => string.Equals(
                    uploadWorksheet.Cells[1, column].Text.Replace(" ", string.Empty),
                    CommittedProjectsColumnHeaders.ProjectSource.Replace(" ", string.Empty),
                    StringComparison.OrdinalIgnoreCase));
            uploadWorksheet.Cells[1, projectSourceColumn].Value = CommittedProjectsColumnHeaders.ProjectSource;
            var budgetColumn = Enumerable.Range(1, uploadWorksheet.Dimension.End.Column)
                .Single(column => string.Equals(
                    uploadWorksheet.Cells[1, column].Text,
                    CommittedProjectsColumnHeaders.Budget,
                    StringComparison.OrdinalIgnoreCase));
            var yearColumn = Enumerable.Range(1, uploadWorksheet.Dimension.End.Column)
                .Single(column => string.Equals(
                    uploadWorksheet.Cells[1, column].Text,
                    CommittedProjectsColumnHeaders.Year,
                    StringComparison.OrdinalIgnoreCase));
            var networkKeyColumn = Enumerable.Range(1, uploadWorksheet.Dimension.End.Column)
                .Single(column => string.Equals(
                    uploadWorksheet.Cells[1, column].Text,
                    TestAttributeNames.BrKey,
                    StringComparison.OrdinalIgnoreCase));
            uploadWorksheet.Cells[2, budgetColumn].Value = "Not A Budget";
            var duplicateRow = uploadWorksheet.Dimension.End.Row + 1;
            uploadWorksheet.Cells[2, 1, 2, uploadWorksheet.Dimension.End.Column]
                .Copy(uploadWorksheet.Cells[duplicateRow, 1]);

            var importer = new CommittedProjectImporter(
                _testUOW,
                HubServiceMocks.Default(),
                TestAttributeNames.BrKey,
                [TestAttributeNames.BrKey, TestAttributeNames.BmsId]);

            try
            {
                // Act
                importer.ImportProjectsFromWorksheet(uploadWorksheet, simulation, "ignored user id");

                // Assert
                var errorFile = Directory.GetFiles(outputFolder, "*.xlsx").Single();
                using var errorWorkbook = new ExcelPackage(new FileInfo(errorFile));

                Assert.Equal(
                    ["Committed Projects", "Import Errors", "Valid Values"],
                    errorWorkbook.Workbook.Worksheets.Select(worksheet => worksheet.Name).ToArray());

                var dataWorksheet = errorWorkbook.Workbook.Worksheets["Committed Projects"];
                for (var row = 1; row <= uploadWorksheet.Dimension.End.Row; row++)
                {
                    for (var column = 1; column <= uploadWorksheet.Dimension.End.Column; column++)
                    {
                        var expectedValue = uploadWorksheet.Cells[row, column].Value;
                        var actualValue = dataWorksheet.Cells[row, column].Value;
                        if (expectedValue is string expectedText && expectedText.Length == 0)
                        {
                            Assert.Null(actualValue);
                        }
                        else
                        {
                            Assert.Equal(expectedValue?.ToString(), actualValue?.ToString());
                        }
                    }
                }
                Assert.Equal("Not A Budget", dataWorksheet.Cells[2, budgetColumn].Text);
                Assert.Equal(OfficeOpenXml.Style.ExcelFillStyle.Solid, dataWorksheet.Cells[2, budgetColumn].Style.Fill.PatternType);
                Assert.Equal(OfficeOpenXml.Style.ExcelFillStyle.Solid, dataWorksheet.Cells[duplicateRow, networkKeyColumn].Style.Fill.PatternType);
                Assert.Equal(OfficeOpenXml.Style.ExcelFillStyle.Solid, dataWorksheet.Cells[duplicateRow, yearColumn].Style.Fill.PatternType);

                var errorWorksheet = errorWorkbook.Workbook.Worksheets["Import Errors"];
                var detailsTable = errorWorksheet.Tables["ImportErrorDetails"];
                Assert.Equal(3, detailsTable.Address.Rows);
                Assert.Contains(
                    errorWorksheet.Cells,
                    cell => cell.Text.Contains("does not match a scenario budget", StringComparison.OrdinalIgnoreCase));
                Assert.Contains(
                    errorWorksheet.Cells,
                    cell => cell.Text.Contains($"{ExcelCellAddress.GetColumnLetter(yearColumn)}{duplicateRow}", StringComparison.OrdinalIgnoreCase));
                Assert.Contains(errorWorksheet.Cells, cell => cell.Hyperlink != null);

                var validValuesWorksheet = errorWorkbook.Workbook.Worksheets["Valid Values"];
                Assert.Contains(validValuesWorksheet.Cells, cell => cell.Text == TestDataForCommittedProjects.InterstateBudgetName);
            }
            finally
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
            }
        }

        [Fact]
        public void ImportErrorWorkbookReportsMixedBadDataWithoutLosingValues()
        {
            // Arrange
            _mockSelectableTreatmentRepo
                .Setup(repository => repository.GetScenarioSelectableTreatmentNames(It.IsAny<Guid>()))
                .Returns(["Rehabilitation", "Maintenance"]);

            var simulation = TestDataForCommittedProjects.TestSimulationDTO();
            simulation.Id = Guid.NewGuid();
            var outputFolder = Path.Combine("CommittedProjects", simulation.Id.ToString());

            using var upload = new ExcelPackage();
            var uploadWorksheet = upload.Workbook.Worksheets.Add("Bad Client Upload", _excelData.Workbook.Worksheets[0]);

            int FindColumn(string header) => Enumerable.Range(1, uploadWorksheet.Dimension.End.Column)
                .Single(column => string.Equals(
                    uploadWorksheet.Cells[1, column].Text.Replace(" ", string.Empty),
                    header.Replace(" ", string.Empty),
                    StringComparison.OrdinalIgnoreCase));

            var networkKeyColumn = FindColumn(TestAttributeNames.BrKey);
            var secondaryKeyColumn = FindColumn(TestAttributeNames.BmsId);
            var treatmentColumn = FindColumn(CommittedProjectsColumnHeaders.Treatment);
            var yearColumn = FindColumn(CommittedProjectsColumnHeaders.Year);
            var budgetColumn = FindColumn(CommittedProjectsColumnHeaders.Budget);
            var costColumn = FindColumn(CommittedProjectsColumnHeaders.Cost);
            var projectSourceColumn = FindColumn(CommittedProjectsColumnHeaders.ProjectSource);
            var categoryColumn = FindColumn(CommittedProjectsColumnHeaders.Category);
            uploadWorksheet.Cells[1, projectSourceColumn].Value = CommittedProjectsColumnHeaders.ProjectSource;

            var badRow = uploadWorksheet.Dimension.End.Row + 1;
            uploadWorksheet.Cells[2, 1, 2, uploadWorksheet.Dimension.End.Column]
                .Copy(uploadWorksheet.Cells[badRow, 1]);
            uploadWorksheet.Cells[badRow, networkKeyColumn].Value = "DOES-NOT-EXIST";
            uploadWorksheet.Cells[badRow, secondaryKeyColumn].Value = null;
            uploadWorksheet.Cells[badRow, treatmentColumn].Value = "Bundle [Rehabilitation|Unknown Treatment]";
            uploadWorksheet.Cells[badRow, yearColumn].Value = 9999;
            uploadWorksheet.Cells[badRow, budgetColumn].Value = "Unknown Budget";
            uploadWorksheet.Cells[badRow, costColumn].Value = "not money";
            uploadWorksheet.Cells[badRow, projectSourceColumn].Value = "not a source";
            uploadWorksheet.Cells[badRow, categoryColumn].Value = "not a category";

            var duplicateRow = badRow + 1;
            uploadWorksheet.Cells[2, 1, 2, uploadWorksheet.Dimension.End.Column]
                .Copy(uploadWorksheet.Cells[duplicateRow, 1]);

            var importer = new CommittedProjectImporter(
                _testUOW,
                HubServiceMocks.Default(),
                TestAttributeNames.BrKey,
                [TestAttributeNames.BrKey, TestAttributeNames.BmsId]);

            try
            {
                // Act
                importer.ImportProjectsFromWorksheet(uploadWorksheet, simulation, "ignored user id");

                // Assert
                var errorFile = Directory.GetFiles(outputFolder, "*.xlsx").Single();
                using var errorWorkbook = new ExcelPackage(new FileInfo(errorFile));
                var dataWorksheet = errorWorkbook.Workbook.Worksheets["Committed Projects"];
                var errorWorksheet = errorWorkbook.Workbook.Worksheets["Import Errors"];
                var detailsTable = errorWorksheet.Tables["ImportErrorDetails"];

                Assert.Equal(10, detailsTable.Address.Rows); // Header plus nine validation issues.
                Assert.Contains(errorWorksheet.Cells, cell => cell.Text.Contains("9 issues", StringComparison.OrdinalIgnoreCase));

                var expectedProblemFragments = new[]
                {
                    "does not match a maintainable asset",
                    "required location value 'BMSID' is blank",
                    "bundle contains treatment names that were not found",
                    "does not match a scenario budget",
                    "not a valid project cost",
                    "not a recognized project source",
                    "not a valid project year",
                    "not a recognized treatment category",
                    "duplicate row"
                };
                foreach (var expectedProblem in expectedProblemFragments)
                {
                    Assert.Contains(
                        errorWorksheet.Cells,
                        cell => cell.Text.Contains(expectedProblem, StringComparison.OrdinalIgnoreCase));
                }

                var badCellColumns = new[]
                {
                    networkKeyColumn,
                    secondaryKeyColumn,
                    treatmentColumn,
                    yearColumn,
                    budgetColumn,
                    costColumn,
                    projectSourceColumn,
                    categoryColumn
                };
                foreach (var column in badCellColumns)
                {
                    Assert.Equal(
                        OfficeOpenXml.Style.ExcelFillStyle.Solid,
                        dataWorksheet.Cells[badRow, column].Style.Fill.PatternType);
                }

                Assert.Equal(
                    OfficeOpenXml.Style.ExcelFillStyle.Solid,
                    dataWorksheet.Cells[duplicateRow, networkKeyColumn].Style.Fill.PatternType);
                Assert.Equal(
                    OfficeOpenXml.Style.ExcelFillStyle.Solid,
                    dataWorksheet.Cells[duplicateRow, yearColumn].Style.Fill.PatternType);
                Assert.Equal(9, errorWorksheet.Cells.Count(cell => cell.Hyperlink != null));

                Assert.Equal("DOES-NOT-EXIST", dataWorksheet.Cells[badRow, networkKeyColumn].Text);
                Assert.Equal(string.Empty, dataWorksheet.Cells[badRow, secondaryKeyColumn].Text);
                Assert.Equal("Bundle [Rehabilitation|Unknown Treatment]", dataWorksheet.Cells[badRow, treatmentColumn].Text);
                Assert.Equal("9999", dataWorksheet.Cells[badRow, yearColumn].Text);
                Assert.Equal("Unknown Budget", dataWorksheet.Cells[badRow, budgetColumn].Text);
                Assert.Equal("not money", dataWorksheet.Cells[badRow, costColumn].Text);
                Assert.Equal("not a source", dataWorksheet.Cells[badRow, projectSourceColumn].Text);
                Assert.Equal("not a category", dataWorksheet.Cells[badRow, categoryColumn].Text);
            }
            finally
            {
                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true);
                }
            }
        }

        [Fact (Skip = "Broken")]
        public void ImportCreatesValidRecordsWithNoTreatment()
        {
            // Arrange
            var userId = "ignored user id";
            List<SectionCommittedProjectDTO> testInput = new List<SectionCommittedProjectDTO>();
            _mockCommittedProjectRepo.Setup(_ => _.UpsertCommittedProjects(It.IsAny<List<SectionCommittedProjectDTO>>()))
                .Callback<List<SectionCommittedProjectDTO>>(_ =>
                {
                    testInput = _;
                });
            var hubService = HubServiceMocks.Default();
            var service = new CommittedProjectService(_testUOW, hubService);
            const string networkKeyAttribute = TestAttributeNames.BrKey;
            // Act - The result is delivered through the callback
            service.ImportCommittedProjectFiles(TestDataForCommittedProjects.SimulationId, _excelData, "GoodFileWithNoTreatment", userId);

            // Assert
            Assert.True(testInput.Count == 3, "Number of comitted projects is wrong");
            Assert.True(testInput[1] is SectionCommittedProjectDTO, "Provided value is not a Section type");
            Assert.True(testInput[1].VerifyLocation(networkKeyAttribute), "Could not verify location");
            Assert.Equal(2023, testInput[1].Year);
            Assert.Equal(TreatmentCategory.CapacityAdding, testInput[1].Category);
            Assert.True(testInput.Any(_ => _.Treatment.Single() == "No Treatment"), "No Treatment was not created");
            var noTreatment = testInput.First(_ => _.Treatment.Single() == "No Treatment");
            Assert.Equal(0, noTreatment.Cost);
            Assert.Equal(2022, noTreatment.Year);
        }
    }

    // Only read the excel file once to prevent conflicts
    public class ExcelAccess : IDisposable
    {
        public ExcelPackage ExcelData { get; set; }

        public ExcelAccess()
        {
            var fileLocation = Path.Combine(Directory.GetCurrentDirectory(), "TestUtils\\Files", "TestCommittedProjects_Good.xlsx");
            using (var stream = File.Open(fileLocation, FileMode.Open, FileAccess.Read))
            {
                ExcelData = new ExcelPackage(stream);
            }
        }

        public void Dispose()
        {
            ExcelData.Dispose();
        }
    }
}
