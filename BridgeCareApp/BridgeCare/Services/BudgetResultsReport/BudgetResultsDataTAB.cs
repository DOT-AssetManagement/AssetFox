using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BridgeCare.Interfaces;
using BridgeCare.Models;
using BridgeCare.Models.SummaryReport.ParametersTAB;
using BridgeCare.Services.CommonData;
using BridgeCare.Services.SummaryReport;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BridgeCare.Services.BudgetResultsReport
{
    public class BudgetResultsDataTAB
    {
        private readonly CommonBridgeData commonBridgeData;
        private readonly IBridgeData bridgeData;
        private readonly ParametersModel parametersModel;
        private readonly ExcelHelper excelHelper;
        private readonly BridgeCareContext dbContext;

        private List<int> SpacerColumnNumbers;

        public BudgetResultsDataTAB(CommonBridgeData commonBridgeData,
             IBridgeData bridgeData, ExcelHelper excelHelper, ParametersModel parametersModel,
            BridgeCareContext dbContext)
        {
            this.commonBridgeData = commonBridgeData ??
                throw new ArgumentNullException(nameof(commonBridgeData));
            this.bridgeData = bridgeData;
            this.parametersModel = parametersModel;
            this.excelHelper = excelHelper;
            this.dbContext = dbContext;
        }
        internal void Fill(ExcelWorksheet worksheet, SimulationModel simulationModel, List<int> simulationYears)
        {
            var commonDataForReport = commonBridgeData.Get(simulationModel, simulationYears, dbContext);
            var simulationDataModels = commonDataForReport.SimulationDataModels;
            var budgetsPerBrKey = commonDataForReport.BudgetsPerBRKeys;

            var treatments = bridgeData.GetTreatments(simulationModel.simulationId, dbContext);

            var BRKeys = simulationDataModels.Select(sm => sm.BRKey).ToList();
            var bridgeDataModels = bridgeData.GetBridgeData(BRKeys, simulationModel, dbContext, parametersModel);

            // Add data to excel.
            var headers = GetHeaders();
            var currentCell = AddHeadersCells(worksheet, headers, simulationYears);

            // Add row next to headers for filters and year numbers for dynamic data. Cover from top, left to right, and bottom set of data.
            using (ExcelRange autoFilterCells = worksheet.Cells[3, 1, currentCell.Row, currentCell.Column - 1])
            {
                autoFilterCells.AutoFilter = true;
            }

            AddBridgeDataModelsCells(worksheet, bridgeDataModels, currentCell);
            AddDynamicDataCells(worksheet, simulationDataModels, bridgeDataModels, currentCell);

            worksheet.Cells.AutoFitColumns();
            var spacerBeforeFirstYear = SpacerColumnNumbers[0] - 6; // to get to the first dark grey highlighted col
            worksheet.Column(spacerBeforeFirstYear).Width = 3;
            foreach (var spacerNumber in SpacerColumnNumbers)
            {
                worksheet.Column(spacerNumber).Width = 3;
            }
            var lastColumn = worksheet.Dimension.Columns + 1;
            worksheet.Column(lastColumn).Width = 3;
        }

        private void AddBridgeDataModelsCells(ExcelWorksheet worksheet, SortedSet<BridgeDataModel> bridgeDataModels, CurrentCell currentCell)
        {
            var rowNo = currentCell.Row;
            var columnNo = currentCell.Column;
            foreach (var bridgeDataModel in bridgeDataModels)
            {
                rowNo++;
                columnNo = 1;
                worksheet.Cells[rowNo, columnNo++].Value = bridgeDataModel.BridgeID;
                worksheet.Cells[rowNo, columnNo++].Value = bridgeDataModel.BRKey;
                worksheet.Cells[rowNo, columnNo].Value = bridgeDataModel.DeckArea;

                // Get NHS record for Parameter TAB
                if (parametersModel.nHSModel.NHS == null || parametersModel.nHSModel.NonNHS == null)
                {
                    switch (bridgeDataModel.NHS)
                    {
                        case "Y":
                            parametersModel.nHSModel.NHS = "Y";
                            break;
                        case "N":
                            parametersModel.nHSModel.NonNHS = "Y";
                            break;
                    }
                }
                // Get BPN data for parameter TAB
                if (!parametersModel.BPNValues.Contains(bridgeDataModel.BPN))
                {
                    parametersModel.BPNValues.Add(bridgeDataModel.BPN);
                }
            }
            currentCell.Row = rowNo;
            currentCell.Column = columnNo + 1; // + 2 to start from Deck Cond
        }

        private void AddDynamicDataCells(ExcelWorksheet worksheet, SortedSet<SimulationDataModel> simulationDataModels,
            SortedSet<BridgeDataModel> bridgeDataModels, CurrentCell currentCell)
        {
            var row = 4; // Data starts here
            var startingRow = row;
            var column = currentCell.Column;
            int totalColumn = 0;
            int totalColumnValue = 0;
            var abbreviatedTreatmentNames = ShortNamesForTreatments.GetShortNamesForTreatments();

            var collectedSet = bridgeDataModels.Zip(simulationDataModels, (x, y) => new { BridgeData = x, SimulationData = y });

            foreach (var entry in collectedSet)
            {
                if (row % 2 == 0)
                {
                    excelHelper.ApplyColor(worksheet.Cells[row, 1, row, worksheet.Dimension.Columns], Color.LightGray);
                }
                column = currentCell.Column;
                var familyId = entry.BridgeData.BridgeFamily;
                var yearsData = entry.SimulationData.YearsData;
                var projectPickByYear = new Dictionary<int, int>();
                // Add work done cells

                for (var index = 1; index < yearsData.Count(); index++)
                {
                    projectPickByYear.Add(yearsData[index].Year, yearsData[index].ProjectPickType);
                }

                // Add Total of count of Work done more than once column cells if "Yes"
                totalColumn = column;

                worksheet.Column(column).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(column).Style.Fill.BackgroundColor.SetColor(Color.Gray);


                // Last Year simulation data
                var lastYearData = yearsData.FirstOrDefault();
                column = AddSimulationYearData(worksheet, row, column, lastYearData, familyId, entry.BridgeData, projectPickByYear);

                // Add all yrs from current year simulation data
                for (var index = 1; index < yearsData.Count(); index++)
                {
                    column = AddSimulationYearData(worksheet, row, column, yearsData[index], familyId, entry.BridgeData, projectPickByYear);
                }
                row++;
            }
            if (totalColumn != 0)
            {
                worksheet.Cells[3, totalColumn].Value = totalColumnValue;
            }
            currentCell.Row = row - 1;
            currentCell.Column = column - 1;
        }

        private int AddSimulationYearData(ExcelWorksheet worksheet, int row, int column, YearsData yearData, int familyId,
            BridgeDataModel bridgeDataModel, Dictionary<int, int> projectPickByYear)
        {
            var familyIdLessThanEleven = familyId < 11;
            if (familyId > 10)
            {
                worksheet.Cells[row, ++column].Value = "N";
                worksheet.Cells[row, ++column].Value = "N";
                worksheet.Cells[row, ++column].Value = "N";

                yearData.Deck = "N";
                yearData.Super = "N";
                yearData.Sub = "N";
            }
            else
            {
                worksheet.Cells[row, ++column].Value = Convert.ToDouble(yearData.Deck);
                worksheet.Cells[row, ++column].Value = Convert.ToDouble(yearData.Super);
                worksheet.Cells[row, ++column].Value = Convert.ToDouble(yearData.Sub);
            }
            if (familyIdLessThanEleven)
            {
                worksheet.Cells[row, ++column].Value = "N";
                yearData.Culv = "N";
                yearData.CulvD = "N";
            }
            else
            {
                worksheet.Cells[row, ++column].Value = Convert.ToDouble(yearData.Culv);
            }

            if (bridgeDataModel.P3 > 0 && yearData.MinC < 5)
            {
                excelHelper.ApplyColor(worksheet.Cells[row, column], Color.Yellow);
                excelHelper.SetTextColor(worksheet.Cells[row, column], Color.Black);
            }
            worksheet.Cells[row, ++column].Value = yearData.MinC < 5 ? "Y" : "N"; //poor

            if (yearData.Year != 0)
            {
                worksheet.Cells[row, ++column].Value = yearData.ProjectPick; // Project Pick
                worksheet.Cells[row, ++column].Value = yearData.Project;
                if (projectPickByYear[yearData.Year] == 2)
                {
                    excelHelper.ApplyColor(worksheet.Cells[row, column], Color.FromArgb(0, 255, 0));
                    excelHelper.SetTextColor(worksheet.Cells[row, column], Color.Black);
                }
                worksheet.Cells[row, ++column].Value = yearData.Cost;
                excelHelper.SetCurrencyFormat(worksheet.Cells[row, column]);
            }
            // Empty column
            column++;
            worksheet.Column(column).Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Column(column).Style.Fill.BackgroundColor.SetColor(Color.Gray);

            return column;
        }

        private List<string> GetHeaders()
        {
            return new List<string>
            {
                "BridgeID",
                "BRKey",
                "Deck Area"
            };
        }
        private CurrentCell AddHeadersCells(ExcelWorksheet worksheet, List<string> headers, List<int> simulationYears)
        {
            int headerRow = 1;
            for (int column = 0; column < headers.Count; column++)
            {
                worksheet.Cells[headerRow, column + 1].Value = headers[column];
            }
            var currentCell = new CurrentCell { Row = headerRow, Column = headers.Count + 1 };
            excelHelper.ApplyBorder(worksheet.Cells[headerRow, 1, headerRow + 1, worksheet.Dimension.Columns]);

            AddDynamicHeadersCells(worksheet, currentCell, simulationYears);
            return currentCell;
        }

        private void AddDynamicHeadersCells(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears)
        {
            var column = currentCell.Column;
            var row = currentCell.Row;
            var initialColumn = column;

            // Add Years Data headers
            column++; // for an empty cell at col number 5
            var simulationHeaderTexts = GetSimulationHeaderTexts();
            worksheet.Cells[row, column].Value = simulationYears[0] - 1;
            column = currentCell.Column;
            column = AddSimulationHeaderTexts(worksheet, column, row, simulationHeaderTexts, simulationHeaderTexts.Count - 3);
            excelHelper.MergeCells(worksheet, row, currentCell.Column + 1, row, column);

            // Empty column
            currentCell.Column = ++column;
            SpacerColumnNumbers = new List<int>();

            foreach (var simulationYear in simulationYears)
            {
                worksheet.Cells[row, ++column].Value = simulationYear;
                column = currentCell.Column;
                column = AddSimulationHeaderTexts(worksheet, column, row, simulationHeaderTexts, simulationHeaderTexts.Count);
                excelHelper.MergeCells(worksheet, row, currentCell.Column + 1, row, column);
                if (simulationYear % 2 != 0)
                {
                    excelHelper.ApplyColor(worksheet.Cells[row, currentCell.Column + 1, row, column], Color.Gray);
                }
                else
                {
                    excelHelper.ApplyColor(worksheet.Cells[row, currentCell.Column + 1, row, column], Color.LightGray);
                }

                worksheet.Column(currentCell.Column).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(currentCell.Column).Style.Fill.BackgroundColor.SetColor(Color.Gray);
                SpacerColumnNumbers.Add(currentCell.Column);

                currentCell.Column = ++column;
            }
            excelHelper.ApplyBorder(worksheet.Cells[row, initialColumn, row + 1, worksheet.Dimension.Columns]);
            currentCell.Row = currentCell.Row + 2;
        }

        private int AddSimulationHeaderTexts(ExcelWorksheet worksheet, int column, int row, List<string> simulationHeaderTexts, int length)
        {
            column++; // to start from column number 5 for the first time. column++ is to leave a space between 2 years data
            for (var index = 0; index < length; index++)
            {
                worksheet.Cells[row + 1, column].Value = simulationHeaderTexts[index];
                excelHelper.ApplyStyle(worksheet.Cells[row + 1, column]);
                column++;
            }

            return column - 1; // to return the last filled column
        }

        private List<string> GetSimulationHeaderTexts()
        {
            return new List<string>
            {
                "Deck Cond",
                "Super Cond",
                "Sub Cond",
                "Culv Cond",
                "Poor",
                "Project Pick",
                "Project",
                "Cost"
            };
        }
    }
}
