using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Utils;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetFoxCore.Services.CommittedProjects
{
    /// <summary>
    /// Handles the import of committed projects from an Excel worksheet.
    /// </summary>
    public class CommittedProjectImporter
    {
        // Dependencies required for importing
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubService _hubService;
        private readonly string _networkKeyField;
        private List<string> _keyFields;

        // Collections for error tracking and project management
        private List<ImportValidationError> _validationErrors = [];
        private Dictionary<(string locationIdentifier, int projectYear, string treatment), SectionCommittedProjectDTO> _projectsPerKey = [];
        private Dictionary<(string locationIdentifier, int projectYear), int> _processedKeys = [];

        private const string DataSheetName = "Committed Projects";
        private const string ErrorSheetName = "Import Errors";
        private const string ValidValuesSheetName = "Valid Values";

        public enum ErrorType
        {
            MissingValue,
            InvalidValue,
            DuplicateEntry,
            AssetNotFound,
            ParsingError,
            GeneralError
        }

        private sealed record ImportCellReference(int Row, int Column)
        {
            public string Address => Row > 0 && Column > 0
                ? ExcelCellAddress.GetColumnLetter(Column) + Row
                : string.Empty;
        }

        private sealed record ImportValidationError(
            ErrorType Type,
            string Code,
            string Severity,
            string ColumnName,
            string CurrentValue,
            string Problem,
            string Resolution,
            ImportCellReference Cell = null,
            List<ImportCellReference> RelatedCells = null)
        {
            public string CellAddress => Cell?.Address ?? string.Empty;
            public string RelatedCellAddresses => string.Join(", ",
                (RelatedCells ?? []).Select(reference => reference.Address).Where(address => !string.IsNullOrEmpty(address)));
        }

        public CommittedProjectImporter(
            IUnitOfWork unitOfWork,
            IHubService hubService,
            string networkKeyField,
            List<string> keyFields)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            _networkKeyField = networkKeyField ?? throw new ArgumentNullException(nameof(networkKeyField));
            _keyFields = keyFields ?? throw new ArgumentNullException(nameof(keyFields));
        }

        public List<SectionCommittedProjectDTO> ImportProjectsFromWorksheet(
            ExcelWorksheet worksheet,
            SimulationDTO simulation,
            string userId)
        {
            // Reset collections for each import
            _validationErrors = [];
            _projectsPerKey = [];
            _processedKeys = [];

            var headers = GetHeadersFromWorksheet(worksheet);
            if (!ValidateWorkSheet(worksheet, _hubService, _networkKeyField, headers, userId))
            {
                SaveErrorExportWorkbook(simulation.Id, worksheet, [], [], []);
                return [];
            }

            //Get Budgets
            var budgets = _unitOfWork.BudgetRepo.GetScenarioBudgets(simulation.Id)
                .ToDictionary(b => b.Name, b => b.Id, StringComparer.OrdinalIgnoreCase);

            //Get Investment Years
            var years = _unitOfWork.InvestmentPlanRepo.GetInvestmentStartAndEndYears(simulation.Id);

            //Get Treatments
            var treatments = _unitOfWork.SelectableTreatmentRepo.GetScenarioSelectableTreatmentNames(simulation.Id);

            // Validate columns once
            var columnHeaders = new List<string>();
            columnHeaders.AddRange(_keyFields);
            columnHeaders.AddRange(CommittedProjectsColumnHeaders.NoKeyHeaders);
            var columnIndices = ValidateAndGetColumnIndices(headers, columnHeaders);

            var locationColumnNames = GetLocationColumnNamesAndKeyColumn(worksheet, headers, out var keyColumn);           

            var maintainableAssetIdsPerLocationId = GetMaintainableAssetsPerLocationId(simulation.NetworkId);
            if (maintainableAssetIdsPerLocationId == null || maintainableAssetIdsPerLocationId.Count == 0)
            {
                var problem = $"No maintainable assets were found for network {simulation.NetworkId}.";
                AddValidationError(
                    ErrorType.AssetNotFound,
                    "CP011",
                    string.Empty,
                    string.Empty,
                    problem,
                    "Confirm that the scenario network contains imported assets before uploading committed projects.");
                HandleValidationError(_hubService, userId, problem);
                SaveErrorExportWorkbook(simulation.Id, worksheet, budgets.Keys, treatments, years);
                return [];
            }

            int startRow = 2; // Assuming headers are in row 1
            int endRow = worksheet.Dimension.End.Row;

            for (int row = startRow; row <= endRow; row++)
            {
                try
                {
                    // Read all cell values for the row
                    var rowValues = new Dictionary<int, object>();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowValues[col] = worksheet.Cells[row, col].Value;
                    }

                    // Skip the row if it is blank
                    if (IsRowBlank(rowValues))
                    {
                        continue;
                    }

                    var project = ProcessRow(
                        row,
                        rowValues,
                        columnIndices,
                        locationColumnNames,
                        maintainableAssetIdsPerLocationId,
                        budgets,
                        treatments,
                        years,
                        simulation.Id);

                    if (project != null)
                    {
                        var key = (project.LocationKeys[_networkKeyField], project.Year, project.ComputedTreatmentString);
                        _projectsPerKey.TryAdd(key, project);
                    }
                }
                catch (Exception)
                {
                    var rowCell = keyColumn > 0 ? new ImportCellReference(row, keyColumn) : null;
                    AddValidationError(
                        ErrorType.GeneralError,
                        "CP012",
                        _networkKeyField,
                        rowCell == null ? string.Empty : GetCellDisplayValue(worksheet, rowCell),
                        $"Row {row} could not be processed.",
                        "Review the values in this row for unsupported or malformed data, then upload the corrected workbook again.",
                        rowCell);
                }
            };

            SaveErrorExportWorkbook(simulation.Id, worksheet, budgets.Keys, treatments, years);

            return _projectsPerKey.Values.ToList();
        }

        private void SaveErrorExportWorkbook(
            Guid simulationId,
            ExcelWorksheet sourceWorksheet,
            IEnumerable<string> budgets,
            IEnumerable<string> treatments,
            int[] years)
        {
            var folderPathForSimulation = Path.Combine("CommittedProjects", simulationId.ToString());
            if (Directory.Exists(folderPathForSimulation))
            {
                Directory.Delete(folderPathForSimulation, true);
            }
            Directory.CreateDirectory(folderPathForSimulation);

            if (!_validationErrors.Any())
            {
                return;
            }

            using var excelPackage = new ExcelPackage();
            CreateDataSheet(excelPackage, sourceWorksheet);
            CreateImportErrorsSheet(excelPackage);
            CreateValidValuesSheet(excelPackage, budgets, treatments, years);

            var filePath = Path.Combine(
                folderPathForSimulation,
                $"CommittedProjectsAudit_Errors_{_validationErrors.Count}.xlsx");
            File.WriteAllBytes(filePath, excelPackage.GetAsByteArray());
        }

        private void CreateDataSheet(ExcelPackage excelPackage, ExcelWorksheet sourceWorksheet)
        {
            var dataSheet = excelPackage.Workbook.Worksheets.Add(DataSheetName);
            if (sourceWorksheet?.Dimension != null)
            {
                for (var row = 1; row <= sourceWorksheet.Dimension.End.Row; row++)
                {
                    for (var column = 1; column <= sourceWorksheet.Dimension.End.Column; column++)
                    {
                        var sourceValue = sourceWorksheet.Cells[row, column].Value;
                        dataSheet.Cells[row, column].Value = sourceValue is string text && text.Length == 0
                            ? null
                            : sourceValue;
                    }
                }

                var header = dataSheet.Cells[1, 1, 1, sourceWorksheet.Dimension.End.Column];
                header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                header.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 78, 121));
                header.Style.Font.Color.SetColor(Color.White);
                header.Style.Font.Bold = true;
                header.AutoFilter = true;
                dataSheet.View.FreezePanes(2, 1);
                dataSheet.Cells.AutoFitColumns();

                for (var column = 1; column <= sourceWorksheet.Dimension.End.Column; column++)
                {
                    dataSheet.Column(column).Width = Math.Min(Math.Max(dataSheet.Column(column).Width, 12), 30);
                }
            }

            foreach (var cellReference in GetAllErrorCellReferences()
                         .Where(reference => sourceWorksheet?.Dimension != null
                             && reference.Row <= sourceWorksheet.Dimension.End.Row
                             && reference.Column <= sourceWorksheet.Dimension.End.Column)
                         .GroupBy(reference => reference.Address)
                         .Select(group => group.First()))
            {
                var cell = dataSheet.Cells[cellReference.Row, cellReference.Column];
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
                cell.Style.Font.Color.SetColor(Color.FromArgb(156, 0, 6));
            }
        }

        private void CreateImportErrorsSheet(ExcelPackage excelPackage)
        {
            var errorSheet = excelPackage.Workbook.Worksheets.Add(ErrorSheetName);
            errorSheet.View.ShowGridLines = false;
            errorSheet.View.FreezePanes(3, 1);

            const int sheetColumnCount = 6;
            errorSheet.Cells[1, 1, 1, sheetColumnCount].Merge = true;
            errorSheet.Cells[1, 1].Value = "Committed Project Import Errors";
            FormatTitle(errorSheet.Cells[1, 1, 1, sheetColumnCount]);

            errorSheet.Cells[2, 1, 2, sheetColumnCount].Merge = true;
            errorSheet.Cells[2, 1].Value =
                "Fix the highlighted cells on the Committed Projects sheet, then upload this workbook again. Click a cell address below to jump to it.";
            errorSheet.Cells[2, 1].Style.WrapText = true;
            errorSheet.Row(2).Height = 28;

            var allCellReferences = GetAllErrorCellReferences().ToList();
            var affectedRows = allCellReferences.Select(reference => reference.Row).Distinct().Count();
            var affectedCells = allCellReferences.Select(reference => reference.Address).Distinct().Count();

            errorSheet.Cells[4, 1, 4, sheetColumnCount].Merge = true;
            errorSheet.Cells[4, 1].Value =
                $"{_validationErrors.Count} {Pluralize(_validationErrors.Count, "issue", "issues")}  |  " +
                $"{affectedRows} affected {Pluralize(affectedRows, "row", "rows")}  |  " +
                $"{affectedCells} affected {Pluralize(affectedCells, "cell", "cells")}";
            FormatSectionHeader(errorSheet.Cells[4, 1, 4, sheetColumnCount]);
            errorSheet.Cells[4, 1].Style.Font.Size = 12;
            errorSheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var groupedErrors = _validationErrors
                .GroupBy(error => new
                {
                    error.Code,
                    error.ColumnName,
                    error.Resolution
                })
                .Select(group => new
                {
                    Occurrences = group.Count(),
                    group.Key.ColumnName,
                    Problem = GetSummaryProblem(group.First()),
                    group.Key.Resolution
                })
                .OrderByDescending(group => group.Occurrences)
                .ThenBy(group => group.ColumnName)
                .Take(10)
                .ToList();

            const int groupedTitleRow = 6;
            errorSheet.Cells[groupedTitleRow, 1, groupedTitleRow, sheetColumnCount].Merge = true;
            errorSheet.Cells[groupedTitleRow, 1].Value = "Most Common Problems";
            FormatSectionHeader(errorSheet.Cells[groupedTitleRow, 1, groupedTitleRow, sheetColumnCount]);

            const int groupedHeaderRow = groupedTitleRow + 1;
            var groupedHeaderBlocks = new[]
            {
                (Start: 1, End: 1, Label: "Occurrences"),
                (Start: 2, End: 2, Label: "Field"),
                (Start: 3, End: 4, Label: "Problem"),
                (Start: 5, End: 6, Label: "How to Fix")
            };
            foreach (var block in groupedHeaderBlocks)
            {
                if (block.End > block.Start)
                {
                    errorSheet.Cells[groupedHeaderRow, block.Start, groupedHeaderRow, block.End].Merge = true;
                }
                errorSheet.Cells[groupedHeaderRow, block.Start].Value = block.Label;
            }
            var groupedHeaderRange = errorSheet.Cells[groupedHeaderRow, 1, groupedHeaderRow, sheetColumnCount];
            groupedHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            groupedHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 110, 140));
            groupedHeaderRange.Style.Font.Color.SetColor(Color.White);
            groupedHeaderRange.Style.Font.Bold = true;

            for (var index = 0; index < groupedErrors.Count; index++)
            {
                var row = groupedHeaderRow + index + 1;
                var groupedError = groupedErrors[index];
                errorSheet.Cells[row, 1].Value = groupedError.Occurrences;
                errorSheet.Cells[row, 2].Value = EmptyAsWorkbookScope(groupedError.ColumnName);
                errorSheet.Cells[row, 3, row, 4].Merge = true;
                errorSheet.Cells[row, 3].Value = groupedError.Problem;
                errorSheet.Cells[row, 5, row, 6].Merge = true;
                errorSheet.Cells[row, 5].Value = groupedError.Resolution;

                var rowRange = errorSheet.Cells[row, 1, row, sheetColumnCount];
                rowRange.Style.WrapText = true;
                rowRange.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                errorSheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (index % 2 == 0)
                {
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
                }
                errorSheet.Row(row).Height = 32;
            }

            var detailTitleRow = groupedHeaderRow + groupedErrors.Count + 2;
            errorSheet.Cells[detailTitleRow, 1, detailTitleRow, sheetColumnCount].Merge = true;
            errorSheet.Cells[detailTitleRow, 1].Value = "Error Details";
            FormatSectionHeader(errorSheet.Cells[detailTitleRow, 1, detailTitleRow, sheetColumnCount]);

            var detailHeaderRow = detailTitleRow + 1;
            var detailHeaders = new[]
            {
                "Cell / Scope", "Field", "Current Value", "What's Wrong", "How to Fix", "Related Cells"
            };
            for (var column = 1; column <= detailHeaders.Length; column++)
            {
                errorSheet.Cells[detailHeaderRow, column].Value = detailHeaders[column - 1];
            }

            var orderedErrors = _validationErrors
                .OrderBy(error => error.Cell?.Row ?? 0)
                .ThenBy(error => error.Cell?.Column ?? 0)
                .ThenBy(error => error.Code)
                .ToList();
            for (var index = 0; index < orderedErrors.Count; index++)
            {
                var row = detailHeaderRow + index + 1;
                var error = orderedErrors[index];
                if (string.IsNullOrEmpty(error.CellAddress))
                {
                    errorSheet.Cells[row, 1].Value = "Workbook";
                }
                else
                {
                    errorSheet.Cells[row, 1].Value = error.CellAddress;
                    errorSheet.Cells[row, 1].Hyperlink = new ExcelHyperLink(
                        $"'{DataSheetName}'!{error.CellAddress}",
                        error.CellAddress);
                    errorSheet.Cells[row, 1].Style.Font.Color.SetColor(Color.FromArgb(5, 99, 193));
                    errorSheet.Cells[row, 1].Style.Font.UnderLine = true;
                }
                errorSheet.Cells[row, 2].Value = EmptyAsWorkbookScope(error.ColumnName);
                errorSheet.Cells[row, 3].Value = DisplayValue(error.CurrentValue);
                errorSheet.Cells[row, 4].Value = error.Problem;
                errorSheet.Cells[row, 5].Value = error.Resolution;
                if (!string.IsNullOrEmpty(error.RelatedCellAddresses))
                {
                    errorSheet.Cells[row, 6].Value = error.RelatedCellAddresses;
                }

                errorSheet.Row(row).Height = 52;
            }

            var detailEndRow = detailHeaderRow + orderedErrors.Count;
            var detailTable = errorSheet.Tables.Add(
                errorSheet.Cells[detailHeaderRow, 1, detailEndRow, detailHeaders.Length],
                "ImportErrorDetails");
            detailTable.TableStyle = TableStyles.Medium2;

            errorSheet.Cells[detailHeaderRow + 1, 3, detailEndRow, detailHeaders.Length].Style.WrapText = true;
            errorSheet.Cells[detailHeaderRow + 1, 3, detailEndRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            errorSheet.Cells[detailHeaderRow + 1, 1, detailEndRow, detailHeaders.Length].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            errorSheet.Column(1).Width = 12;
            errorSheet.Column(2).Width = 18;
            errorSheet.Column(3).Width = 22;
            errorSheet.Column(4).Width = 30;
            errorSheet.Column(5).Width = 34;
            errorSheet.Column(6).Width = 16;
        }

        private static string GetSummaryProblem(ImportValidationError error) => error.Code switch
        {
            "CP001" => "A required value is blank.",
            "CP002" => "The asset identifier does not match an asset in this scenario's network.",
            "CP003" => "The project source is not recognized.",
            "CP004" => "The budget does not match a scenario budget.",
            "CP005" => "The project cost is not a valid number.",
            "CP006" => "The treatment or treatment bundle is not valid.",
            "CP007" => "The project year is outside the scenario investment period or is not a whole number.",
            "CP008" => "The treatment category is not recognized.",
            "CP009" => "More than one project uses the same asset and year.",
            "CP010" => "A required column is missing from row 1.",
            "CP011" => "The scenario network has no maintainable assets.",
            "CP012" => "A row could not be processed.",
            "CP013" => "The workbook structure is invalid.",
            _ => error.Problem
        };

        private static string Pluralize(int count, string singular, string plural) =>
            count == 1 ? singular : plural;

        private static void CreateValidValuesSheet(
            ExcelPackage excelPackage,
            IEnumerable<string> budgets,
            IEnumerable<string> treatments,
            int[] years)
        {
            var validValuesSheet = excelPackage.Workbook.Worksheets.Add(ValidValuesSheetName);
            validValuesSheet.View.ShowGridLines = false;
            validValuesSheet.View.FreezePanes(4, 1);
            validValuesSheet.Cells[1, 1, 1, 9].Merge = true;
            validValuesSheet.Cells[1, 1].Value = "Valid Values for This Import";
            FormatTitle(validValuesSheet.Cells[1, 1, 1, 9]);
            validValuesSheet.Cells[2, 1, 2, 9].Merge = true;
            validValuesSheet.Cells[2, 1].Value =
                "Use these scenario-specific values when correcting the Committed Projects sheet. This sheet is informational and is not imported.";

            var budgetValues = (budgets ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList();
            var treatmentValues = (treatments ?? []).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList();
            var categoryValues = Enum.GetNames<TreatmentCategory>().OrderBy(value => value).ToList();
            var sourceValues = Enum.GetNames<ProjectSourceDTO>().OrderBy(value => value).ToList();

            AddReferenceList(validValuesSheet, 1, "Budgets", budgetValues);
            AddReferenceList(validValuesSheet, 3, "Treatments", treatmentValues);
            AddReferenceList(validValuesSheet, 5, "Categories", categoryValues);
            AddReferenceList(validValuesSheet, 7, "Project Sources", sourceValues);

            validValuesSheet.Cells[3, 9].Value = "Investment Years";
            FormatSectionHeader(validValuesSheet.Cells[3, 9]);
            if (years?.Length >= 2)
            {
                validValuesSheet.Cells[4, 9].Value = $"Start: {years[0]}";
                validValuesSheet.Cells[5, 9].Value = $"End: {years[1]}";
            }
            else
            {
                validValuesSheet.Cells[4, 9].Value = "Not available";
            }

            foreach (var column in new[] { 1, 3, 5, 7, 9 })
            {
                validValuesSheet.Column(column).Width = 28;
            }
        }

        private static void AddReferenceList(
            ExcelWorksheet worksheet,
            int column,
            string heading,
            IReadOnlyList<string> values)
        {
            worksheet.Cells[3, column].Value = heading;
            FormatSectionHeader(worksheet.Cells[3, column]);
            if (!values.Any())
            {
                worksheet.Cells[4, column].Value = "No values available";
                return;
            }

            for (var index = 0; index < values.Count; index++)
            {
                worksheet.Cells[index + 4, column].Value = values[index];
            }
        }

        private static void FormatTitle(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 78, 121));
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 16;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Worksheet.Row(range.Start.Row).Height = 28;
        }

        private static void FormatSectionHeader(ExcelRange range)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
            range.Style.Font.Color.SetColor(Color.FromArgb(31, 78, 121));
            range.Style.Font.Bold = true;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private static string DisplayValue(string value) =>
            string.IsNullOrWhiteSpace(value) ? "(blank)" : value;

        private static string EmptyAsWorkbookScope(string value) =>
            string.IsNullOrWhiteSpace(value) ? "Workbook" : value;

        private IEnumerable<ImportCellReference> GetAllErrorCellReferences() =>
            _validationErrors.SelectMany(error =>
                (error.Cell == null ? Enumerable.Empty<ImportCellReference>() : new[] { error.Cell })
                .Concat(error.RelatedCells ?? []));

        private static string GetCellDisplayValue(ExcelWorksheet worksheet, ImportCellReference reference) =>
            worksheet?.Cells[reference.Row, reference.Column].Text ?? string.Empty;

        /// <summary>
        /// Processes a single row in the worksheet and creates a committed project DTO.
        /// </summary>
        private SectionCommittedProjectDTO ProcessRow(
            int row,
            Dictionary<int, object> rowValues, 
            Dictionary<string, int> columnIndices,
            Dictionary<int, string> locationColumnNames,
            Dictionary<string, Guid> maintainableAssetIdsPerLocationId,
            Dictionary<string, Guid> budgets,
            List<string> treatments,
            int[] years,
            Guid simulationId)
        {
            
            // Extract key components
            var locationId = SafeGetLocationIdentifier(rowValues, columnIndices, _networkKeyField, row);
            var projectYear = SafeGetProjectYear(rowValues, columnIndices, years, CommittedProjectsColumnHeaders.Year, row);
            var treatment = SafeGetTreatment(rowValues, columnIndices, treatments, CommittedProjectsColumnHeaders.Treatment, row);

            var key = (locationIdentifier: locationId, projectYear);
            var keyColumnIndex = columnIndices[_networkKeyField];
            var yearColumnIndex = columnIndices[CommittedProjectsColumnHeaders.Year];

            // Check for duplicates
            if (_processedKeys.TryGetValue(key, out var originalRow))
            {
                var relatedCells = new List<ImportCellReference>
                {
                    new(row, yearColumnIndex),
                    new(originalRow, keyColumnIndex),
                    new(originalRow, yearColumnIndex)
                }.Where(reference => reference.Column > 0).ToList();

                AddValidationError(
                    ErrorType.DuplicateEntry,
                    "CP009",
                    _networkKeyField,
                    locationId,
                    $"This asset and project year duplicate row {originalRow}.",
                    "Keep only one project for this asset and year, or change the asset identifier or year.",
                    new ImportCellReference(row, keyColumnIndex),
                    relatedCells);

                return null; // Skip further processing
            }

            _processedKeys[key] = row;

            var assetId = ValidateLocationIdHasAssets(maintainableAssetIdsPerLocationId, locationId, row, keyColumnIndex);
            var locationInformation = BuildLocationInformation(locationColumnNames, rowValues, assetId, row);        

            return new SectionCommittedProjectDTO
            {
                Id = Guid.NewGuid(),
                SimulationId = simulationId,
                ScenarioBudgetId = SafeGetBudgetId(budgets, rowValues, columnIndices, CommittedProjectsColumnHeaders.Budget, row),
                LocationKeys = locationInformation,
                Treatment = treatment,
                Year = projectYear,
                ProjectSource = SafeGetProjectSource(rowValues, columnIndices, CommittedProjectsColumnHeaders.ProjectSource, row),
                ProjectId = SafeGetProjectSourceId(rowValues, columnIndices, CommittedProjectsColumnHeaders.ProjectId),
                Cost = SafeGetProjectCost(rowValues, columnIndices, CommittedProjectsColumnHeaders.Cost, row),
                Category = SafeGetTreatmentCategory(rowValues, columnIndices, CommittedProjectsColumnHeaders.Category, row)
            };
        }

        private bool ValidateWorkSheet(
            ExcelWorksheet worksheet,
            IHubService _hubService,
            string _networkKeyField,
            List<string> headers,
            string userId)
        {
            // Validate worksheet existence
            if (worksheet == null)
            {
                const string message = "Invalid spreadsheet: No worksheet found.";
                AddValidationError(
                    ErrorType.GeneralError,
                    "CP013",
                    string.Empty,
                    string.Empty,
                    message,
                    $"Add a worksheet named '{DataSheetName}' containing the committed-project headers and data.");
                HandleValidationError(_hubService, userId, message);
                return false;
            }

            // Validate worksheet dimensions
            if (worksheet.Dimension == null || worksheet.Dimension.End.Row < 2)
            {
                const string message = "Invalid spreadsheet: The worksheet must contain column headers and at least one data row.";
                AddValidationError(
                    ErrorType.GeneralError,
                    "CP013",
                    string.Empty,
                    string.Empty,
                    message,
                    "Add the required headers to row 1 and at least one committed project beginning on row 2.");
                HandleValidationError(_hubService, userId, message);
                return false;
            }

            if (!_keyFields.Contains(_networkKeyField, StringComparer.OrdinalIgnoreCase))
            {
                var message = $"Invalid network key field configuration: {_networkKeyField}.";
                AddValidationError(
                    ErrorType.GeneralError,
                    "CP013",
                    _networkKeyField,
                    string.Empty,
                    message,
                    "Ask an administrator to verify the network key-field configuration before importing this workbook.");
                HandleValidationError(_hubService, userId, message);
                return false;
            }

            // Validate key headers
            var missingKeyHeaders = _keyFields
                .Where(column => !headers.Contains(column, StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var missingHeader in missingKeyHeaders)
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP010",
                    missingHeader,
                    string.Empty,
                    $"Required location column '{missingHeader}' is missing from row 1.",
                    $"Add a column named '{missingHeader}' to row 1 and provide a value for each project row.");
            }

            if (missingKeyHeaders.Count != 0)
            {
                var message = $"Invalid spreadsheet: Missing required key columns: {string.Join(", ", missingKeyHeaders)}.";
                HandleValidationError(_hubService, userId, message);
                return false;
            }

            return true;
        }                

        private static void HandleValidationError(IHubService _hubService, string userId, string message)
        {
            _hubService.SendRealTimeMessage(userId, HubConstant.BroadcastWarning, message);
        }

        private Guid ValidateLocationIdHasAssets(
            Dictionary<string, Guid> maintainableAssetIdsPerLocationId,
            string locationId,
            int row,
            int columnIndex)
        {
            if (!maintainableAssetIdsPerLocationId.TryGetValue(locationId, out var assetId))
            {
                AddValidationError(
                    ErrorType.AssetNotFound,
                    "CP002",
                    _networkKeyField,
                    locationId,
                    $"'{locationId}' does not match a maintainable asset in this scenario's network.",
                    "Correct the asset identifier so it exactly matches an existing network asset.",
                    new ImportCellReference(row, columnIndex));
            }

            return assetId;
        }

        private void AddValidationError(
            ErrorType errorType,
            string code,
            string columnName,
            string currentValue,
            string problem,
            string resolution,
            ImportCellReference cell = null,
            IEnumerable<ImportCellReference> relatedCells = null)
        {
            _validationErrors.Add(new ImportValidationError(
                errorType,
                code,
                "Error",
                columnName,
                currentValue,
                problem,
                resolution,
                cell,
                relatedCells?.ToList() ?? []));
        }


        private Dictionary<int, string> GetLocationColumnNamesAndKeyColumn(
            ExcelWorksheet worksheet,
            List<string> headers,
            out int keyColumn)
        {
            var locationColumnNames = new Dictionary<int, string>();
            keyColumn = -1;
            _keyFields = _keyFields.Intersect(headers, StringComparer.OrdinalIgnoreCase).ToList();

            for (var column = 1; column <= worksheet.Dimension.Columns; column++)
            {
                var columnName = worksheet.GetCellValue<string>(1, column) ?? string.Empty;
                locationColumnNames[column] = columnName;

                if (string.Equals(columnName, _networkKeyField, StringComparison.OrdinalIgnoreCase))
                {
                    keyColumn = column;
                }
            }

            if (keyColumn == -1)
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP010",
                    _networkKeyField,
                    string.Empty,
                    $"Required network-key column '{_networkKeyField}' was not found in row 1.",
                    $"Add a column named '{_networkKeyField}' to row 1.");
            }

            return locationColumnNames;
        }

        private Dictionary<string, string> BuildLocationInformation(
            Dictionary<int, string> locationColumnNames,
            Dictionary<int, object> rowValues,
            Guid assetId,
            int row)
        {
            var locationInformation = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ID"] = assetId.ToString()
            };

            foreach (var kvp in locationColumnNames)
            {
                var column = kvp.Key;
                var columnName = kvp.Value;
                var value = rowValues.TryGetValue(column, out var cellValue) && cellValue != null
                    ? cellValue.ToString()
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(value)
                    && _keyFields.Contains(columnName, StringComparer.OrdinalIgnoreCase)
                    && !string.Equals(columnName, _networkKeyField, StringComparison.OrdinalIgnoreCase))
                {
                    AddValidationError(
                        ErrorType.MissingValue,
                        "CP001",
                        columnName,
                        string.Empty,
                        $"Required location value '{columnName}' is blank.",
                        $"Enter the '{columnName}' value for this network asset.",
                        new ImportCellReference(row, column));
                }

                locationInformation[columnName] = value;
            }

            return locationInformation;
        }

        /// <summary>
        /// Retrieves headers from the first row of the worksheet.
        /// </summary>
        private static List<string> GetHeadersFromWorksheet(ExcelWorksheet worksheet)
        {
            if (worksheet?.Dimension == null)
            {
                return [];
            }

            return worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                .Select(cell => cell.GetValue<string>())
                .Where(header => !string.IsNullOrEmpty(header))
                .ToList();
        }

        /// <summary>
        /// Retrieves a mapping of location identifiers to maintainable asset IDs.
        /// </summary>
        private Dictionary<string, Guid> GetMaintainableAssetsPerLocationId(Guid networkId)
        {
            var assets = _unitOfWork.MaintainableAssetRepo.GetAllInNetworkWithLocations(networkId);
            if (!assets.Any())
            {
                return null;
            }

            return assets.ToDictionary(
                asset => asset.Location.LocationIdentifier,
                asset => asset.Id,
                StringComparer.OrdinalIgnoreCase);
        }

        private string SafeGetLocationIdentifier(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName,
            int row)
        {

            var columnIndex = columnIndices[columnName];
            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var identifier = cellValue.ToString().Trim();
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    AddValidationError(
                        ErrorType.MissingValue,
                        "CP001",
                        columnName,
                        string.Empty,
                        "The required network asset identifier is blank.",
                        $"Enter the value of an existing network asset in '{columnName}'.",
                        new ImportCellReference(row, columnIndex));
                }
                return identifier;
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "The required network asset identifier is blank.",
                    $"Enter the value of an existing network asset in '{columnName}'.",
                    new ImportCellReference(row, columnIndex));
                return string.Empty;
            }
        }   
                
        private string SafeGetProjectSourceId(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return string.Empty; //default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var sourceId = cellValue.ToString();
                return string.IsNullOrWhiteSpace(sourceId)
                    ? string.Empty
                    : sourceId.Trim();
            }
            else
            {
                //AddValidationError(ErrorType.MissingValue, $"Row {row}, Column {columnIndex} ('{columnName}'): Project Source ID is missing.");
                return string.Empty;
            }
        }

        private ProjectSourceDTO SafeGetProjectSource(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName,
            int row)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return ProjectSourceDTO.None; // Default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var projectSourceValue = cellValue.ToString();
                if (Enum.TryParse(projectSourceValue, true, out ProjectSourceDTO projectSource)
                    && Enum.IsDefined(projectSource))
                {
                    return projectSource;
                }
                else
                {
                    AddValidationError(
                        ErrorType.InvalidValue,
                        "CP003",
                        columnName,
                        projectSourceValue,
                        $"'{DisplayValue(projectSourceValue)}' is not a recognized project source.",
                        $"Replace it with a project source listed on the '{ValidValuesSheetName}' sheet.",
                        new ImportCellReference(row, columnIndex));
                    return ProjectSourceDTO.None;
                }
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Project source is required but blank.",
                    $"Enter a project source listed on the '{ValidValuesSheetName}' sheet.",
                    new ImportCellReference(row, columnIndex));
                return ProjectSourceDTO.None;
            }
        }


        private Guid? SafeGetBudgetId(
            Dictionary<string, Guid> budgets,
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName,
            int row)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return Guid.Empty; // Default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var budgetName = cellValue.ToString();
                if (budgets.TryGetValue(budgetName, out var budgetId))
                {
                    return budgetId;
                }
                else
                {
                    AddValidationError(
                        ErrorType.InvalidValue,
                        "CP004",
                        columnName,
                        budgetName,
                        $"'{DisplayValue(budgetName)}' does not match a scenario budget.",
                        $"Replace it with a budget listed on the '{ValidValuesSheetName}' sheet.",
                        new ImportCellReference(row, columnIndex));
                    return Guid.Empty;
                }
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Budget is required but blank.",
                    $"Enter a budget listed on the '{ValidValuesSheetName}' sheet.",
                    new ImportCellReference(row, columnIndex));
                return Guid.Empty;
            }
        }


        private double SafeGetProjectCost(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName,
            int row)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return 0.0; // Default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var costValue = cellValue.ToString().Replace("$", "").Replace(",", "");
                if (double.TryParse(costValue, out double doubleVal))
                {
                    return doubleVal;
                }
                else
                {
                    AddValidationError(
                        ErrorType.InvalidValue,
                        "CP005",
                        columnName,
                        cellValue.ToString(),
                        $"'{DisplayValue(cellValue.ToString())}' is not a valid project cost.",
                        "Enter a numeric amount, optionally formatted as currency (for example, 125000 or $125,000).",
                        new ImportCellReference(row, columnIndex));
                    return 0.0;
                }
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Project cost is required but blank.",
                    "Enter the numeric cost of the committed project.",
                    new ImportCellReference(row, columnIndex));
                return 0.0;
            }
        }


        private string[] SafeGetTreatment(
             Dictionary<int, object> rowValues,
             Dictionary<string, int> columnIndices,
             List<string> treatments,
             string columnName,
             int row)
        {
            var result = new List<string>();

            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                result.Add("Default Treatment");
                return getResultArray();
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var treatment = cellValue.ToString().Trim();
                if (string.IsNullOrWhiteSpace(treatment))
                {
                    AddValidationError(
                        ErrorType.MissingValue,
                        "CP001",
                        columnName,
                        string.Empty,
                        "Treatment is required but blank.",
                        $"Enter a treatment listed on the '{ValidValuesSheetName}' sheet.",
                        new ImportCellReference(row, columnIndex));
                    result.Add("Default Treatment");
                    return getResultArray();
                }

                if (treatment.Contains("Bundle", StringComparison.OrdinalIgnoreCase))
                {
                    // Match contents inside brackets: [Treatment A|Treatment B]
                    var matches = Regex.Matches(treatment, @"\[(.*?)\]");
                    if (matches.Count == 0)
                    {
                        AddValidationError(
                            ErrorType.InvalidValue,
                            "CP006",
                            columnName,
                            treatment,
                            "The treatment bundle is malformed and contains no bracketed treatment list.",
                            "Use the bundle format 'Bundle [Treatment A|Treatment B]' with valid treatment names.",
                            new ImportCellReference(row, columnIndex));
                        result.Add("Default Treatment");
                        return getResultArray();
                    }

                    var normalizedMatches = new List<string>();
                    foreach (Match match in matches)
                    {
                        var matchedTreatments = match.Groups[1].Value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        normalizedMatches.AddRange(matchedTreatments);
                    }

                    var unknownTreatments = new List<string>();
                    foreach (var name in normalizedMatches)
                    {
                        var matchingTreatment = treatments.FirstOrDefault(t => string.Equals(t.Trim(), name, StringComparison.OrdinalIgnoreCase));
                        if (matchingTreatment != null)
                        {
                            result.Add(matchingTreatment);
                        }
                        else
                        {
                            unknownTreatments.Add(name);
                        }
                    }

                    if (normalizedMatches.Count == 0 || unknownTreatments.Count != 0)
                    {
                        var problem = normalizedMatches.Count == 0
                            ? "The treatment bundle does not contain any treatment names."
                            : $"The bundle contains treatment names that were not found: {string.Join(", ", unknownTreatments)}.";
                        AddValidationError(
                            ErrorType.InvalidValue,
                            "CP006",
                            columnName,
                            treatment,
                            problem,
                            $"Use only treatment names listed on the '{ValidValuesSheetName}' sheet.",
                            new ImportCellReference(row, columnIndex));
                    }

                    if (result.Count == 0)
                    {
                        result.Add("Default Treatment");
                    }

                    return getResultArray();
                }

                // Check if treatment exists in the provided treatments list 
                var matchingSingleTreatment = treatments
                    .FirstOrDefault(t => string.Equals(t.Trim(), treatment, StringComparison.OrdinalIgnoreCase));

                if (matchingSingleTreatment != null)
                {
                    result.Add(matchingSingleTreatment);
                    return getResultArray();
                }

                AddValidationError(
                    ErrorType.InvalidValue,
                    "CP006",
                    columnName,
                    treatment,
                    $"'{treatment}' does not match a scenario treatment.",
                    $"Replace it with a treatment listed on the '{ValidValuesSheetName}' sheet.",
                    new ImportCellReference(row, columnIndex));
                result.Add("Default Treatment");
                return getResultArray();
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Treatment is required but blank.",
                    $"Enter a treatment listed on the '{ValidValuesSheetName}' sheet.",
                    new ImportCellReference(row, columnIndex));
                result.Add("Default Treatment");
                return getResultArray();
            }

            string[] getResultArray() => result.ToArray();
        }


        private int SafeGetProjectYear(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            int[] years,
            string columnName,
            int row)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return 0; //default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var projectYearStr = cellValue.ToString();
                if (int.TryParse(projectYearStr, out var intValue)
                    && years?.Length >= 2
                    && intValue >= years[0]
                    && intValue <= years[1])
                {
                    return intValue;
                }
                else
                {
                    var allowedYears = years?.Length >= 2
                        ? $"between {years[0]} and {years[1]}"
                        : "within the scenario investment period";
                    AddValidationError(
                        ErrorType.InvalidValue,
                        "CP007",
                        columnName,
                        projectYearStr,
                        $"'{DisplayValue(projectYearStr)}' is not a valid project year {allowedYears}.",
                        $"Enter a whole-number year {allowedYears}.",
                        new ImportCellReference(row, columnIndex));
                    return 0;
                }
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Project year is required but blank.",
                    "Enter a whole-number year within the scenario investment period.",
                    new ImportCellReference(row, columnIndex));
                return 0;
            }
        }

        private TreatmentCategory SafeGetTreatmentCategory(
            Dictionary<int, object> rowValues,
            Dictionary<string, int> columnIndices,
            string columnName,
            int row)
        {
            var columnIndex = columnIndices[columnName];
            if (columnIndex == -1)
            {
                return TreatmentCategory.Other; // Default value
            }

            if (rowValues.TryGetValue(columnIndex, out var cellValue) && cellValue != null)
            {
                var categoryValue = cellValue.ToString();
                if (Enum.TryParse(categoryValue, true, out TreatmentCategory category)
                    && Enum.IsDefined(category))
                {
                    return category;
                }
                else
                {
                    AddValidationError(
                        ErrorType.InvalidValue,
                        "CP008",
                        columnName,
                        categoryValue,
                        $"'{DisplayValue(categoryValue)}' is not a recognized treatment category.",
                        $"Replace it with a category listed on the '{ValidValuesSheetName}' sheet.",
                        new ImportCellReference(row, columnIndex));
                    return TreatmentCategory.Other;
                }
            }
            else
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP001",
                    columnName,
                    string.Empty,
                    "Treatment category is required but blank.",
                    $"Enter a category listed on the '{ValidValuesSheetName}' sheet.",
                    new ImportCellReference(row, columnIndex));
                return TreatmentCategory.Other;
            }
        }


        private Dictionary<string, int> ValidateAndGetColumnIndices(List<string> headers, List<string> requiredColumns)
        {
            var columnIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var missingColumns = new List<string>();

            foreach (var column in requiredColumns)
            {
                var index = headers.FindIndex(h => h.Equals(column, StringComparison.OrdinalIgnoreCase));
                if (index != -1)
                {
                    columnIndices[column] = index + 1; // Excel columns are 1-based
                }
                else
                {
                    missingColumns.Add(column);
                    columnIndices[column] = -1; // Indicate column not found
                }
            }

            missingColumns.Remove(CommittedProjectsColumnHeaders.ProjectId);

            foreach (var missingColumn in missingColumns)
            {
                AddValidationError(
                    ErrorType.MissingValue,
                    "CP010",
                    missingColumn,
                    string.Empty,
                    $"Required project column '{missingColumn}' is missing from row 1.",
                    $"Add a column named '{missingColumn}' to row 1 and provide a value for each project row.");
            }

            return columnIndices;
        }

        private static bool IsRowBlank(Dictionary<int, object> rowValues)
        {
            return rowValues.All(kvp => kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString()));
        }
    }
}
