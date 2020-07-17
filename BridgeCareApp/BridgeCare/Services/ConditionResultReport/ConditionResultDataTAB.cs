using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using BridgeCare.DataAccessLayer;
using BridgeCare.Interfaces;
using BridgeCare.Models;
using OfficeOpenXml;

namespace BridgeCare.Services.ConditionResultReport
{
    public class ConditionResultDataTAB
    {
        enum HeaderLabel { Good, Fair, Poor };
        private readonly BridgeWorkSummaryCommon bridgeWorkSummaryCommon;
        private readonly ExcelHelper excelHelper;
        private readonly BridgeWorkSummaryComputationHelper bridgeWorkSummaryComputationHelper;
        private readonly IBridgeWorkSummaryData bridgeWorkSummaryData;

        public ConditionResultDataTAB(BridgeWorkSummaryCommon bridgeWorkSummaryCommon, BridgeWorkSummaryComputationHelper bridgeWorkSummaryComputationHelper,
            ExcelHelper excelHelper, IBridgeWorkSummaryData bridgeWorkSummaryData)
        {
            this.bridgeWorkSummaryCommon = bridgeWorkSummaryCommon;
            this.excelHelper = excelHelper;
            this.bridgeWorkSummaryComputationHelper = bridgeWorkSummaryComputationHelper;
            this.bridgeWorkSummaryData = bridgeWorkSummaryData ?? throw new ArgumentNullException(nameof(bridgeWorkSummaryData));
        }
        public ChartRowsModel Fill(ExcelWorksheet worksheet, SortedSet<SimulationDataModel> simulationDataModels, List<int> simulationYears, BridgeCareContext dbContext,
            int simulationId)
        {
            var currentCell = new CurrentCell { Row = 1, Column = 1 };
            var chartRowsModel = new ChartRowsModel();
            chartRowsModel.TotalDeckAreaSectionYearsRow = FillTotalDeckAreaSection(worksheet, currentCell, simulationYears, simulationDataModels);

            chartRowsModel.TotalDeckAreaPercentYearsRow = FillTotalDeckAreaPercent(worksheet, currentCell, simulationYears,
                chartRowsModel.TotalDeckAreaSectionYearsRow + 1);

            var yearlyBudgetAmounts = bridgeWorkSummaryData.GetYearlyBudgetAmounts(simulationId, simulationYears, dbContext);
            var budgetTotalRow = FillTotalBudgetSection(worksheet, currentCell, simulationYears, yearlyBudgetAmounts);
            worksheet.Cells.AutoFitColumns();
            chartRowsModel.TotalBridgeCareBudgetPerYear = budgetTotalRow;
            return chartRowsModel;
        }

        private int FillTotalDeckAreaSection(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, SortedSet<SimulationDataModel> simulationDataModels)
        {
            bridgeWorkSummaryCommon.AddBridgeHeaders(worksheet, currentCell, simulationYears, "Total Deck Area", true);
            var totalDeckAreaSectionYearsRow = currentCell.Row;
            AddDetailsForTotalDeckArea(worksheet, currentCell, simulationYears, simulationDataModels);
            return totalDeckAreaSectionYearsRow;
        }

        private void AddDetailsForTotalDeckArea(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, SortedSet<SimulationDataModel> simulationDataModels)
        {
            int startRow, startColumn, row, column;
            bridgeWorkSummaryCommon.InitializeLabelCells(worksheet, currentCell, out startRow, out startColumn, out row, out column);
            AddTotalDeckArea(worksheet, simulationDataModels, startRow, column, 0);
            foreach (var year in simulationYears)
            {
                row = startRow;
                column = ++column;
                AddTotalDeckArea(worksheet, simulationDataModels, row, column, year);
            }
            excelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row + 2, column]);
            excelHelper.SetCustomFormat(worksheet.Cells[startRow, startColumn + 1, row + 2, column], "Number");
            bridgeWorkSummaryCommon.UpdateCurrentCell(currentCell, row + 3, column);
        }

        private void AddTotalDeckArea(ExcelWorksheet worksheet, SortedSet<SimulationDataModel> simulationDataModels, int row, int column, int year)
        {
            var goodCount = bridgeWorkSummaryComputationHelper.CalculateTotalGoodDeckArea(simulationDataModels, year);
            worksheet.Cells[row, column].Value = Convert.ToInt32(goodCount);

            var poorCount = bridgeWorkSummaryComputationHelper.CalculateTotalPoorDeckArea(simulationDataModels, year);
            worksheet.Cells[row + 2, column].Value = poorCount;

            worksheet.Cells[row + 1, column].Value = bridgeWorkSummaryComputationHelper.CalculateTotalDeckArea(simulationDataModels) - (goodCount + poorCount);
        }

        private int FillTotalBudgetSection(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, List<double>> yearlyBudgetAmounts)
        {
            bridgeWorkSummaryCommon.AddBridgeHeaders(worksheet, currentCell, simulationYears, "Total Budget", true);
            worksheet.Cells[currentCell.Row, simulationYears.Count + 3].Value = "Total Analysis Budget (all year)";
            excelHelper.ApplyStyle(worksheet.Cells[currentCell.Row, simulationYears.Count + 3]);
            excelHelper.ApplyBorder(worksheet.Cells[currentCell.Row, simulationYears.Count + 3]);
            var budgetTotalRow = AddDetailsForTotalBudget(worksheet, simulationYears, currentCell, yearlyBudgetAmounts);
            return budgetTotalRow;
        }

        private int AddDetailsForTotalBudget(ExcelWorksheet worksheet, List<int> simulationYears, CurrentCell currentCell, Dictionary<int, List<double>> yearlyBudgetAmounts)
        {
            int startRow, startColumn, row, column;
            bridgeWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);
            int budgetTotalRow = 0;
            worksheet.Cells[row++, column].Value = Properties.Resources.TotalBridgeCareBudget;
            column++;
            worksheet.Cells[startRow, column].Value = 0; // Adding dummy value to make graph
            var fromColumn = column + 1;
            foreach (var year in simulationYears)
            {
                row = startRow;
                column = ++column;

                worksheet.Cells[row, column].Value = yearlyBudgetAmounts.ContainsKey(year) ? yearlyBudgetAmounts[year].Sum() : 0;
                budgetTotalRow = row;
            }
            worksheet.Cells[row, column + 1].Formula = "SUM(" + worksheet.Cells[row, fromColumn - 1, row, column] + ")";

            excelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column + 1]);
            excelHelper.SetCustomFormat(worksheet.Cells[startRow, fromColumn, row, column + 1], "NegativeCurrency");
            excelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column], Color.FromArgb(84, 130, 53));
            excelHelper.SetTextColor(worksheet.Cells[startRow, fromColumn, row, column], Color.White);
            bridgeWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, column);
            return budgetTotalRow;
        }

        private int FillTotalDeckAreaPercent(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, int dataStartRow)
        {
            bridgeWorkSummaryCommon.AddBridgeHeaders(worksheet, currentCell, simulationYears, "Total Deck Area Percentage", true);
            var totalDeckAreaPercentYearsRow = currentCell.Row;
            AddDetailsForTotalBridgeAndDeckPercent(worksheet, currentCell, simulationYears, dataStartRow);
            return totalDeckAreaPercentYearsRow;
        }

        private void AddDetailsForTotalBridgeAndDeckPercent(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, int dataStartRow)
        {
            int startRow, startColumn, row, column;
            bridgeWorkSummaryCommon.InitializeLabelCells(worksheet, currentCell, out startRow, out startColumn, out row, out column);
            for (var index = 0; index <= simulationYears.Count; index++)
            {
                var sumFormula = "SUM(" + worksheet.Cells[dataStartRow, column, dataStartRow + 2, column] + ")";
                worksheet.Cells[startRow, column].Formula = worksheet.Cells[dataStartRow, column] + "/" + sumFormula;
                worksheet.Cells[startRow + 1, column].Formula = worksheet.Cells[dataStartRow + 1, column] + "/" + sumFormula;
                worksheet.Cells[startRow + 2, column].Formula = worksheet.Cells[dataStartRow + 2, column] + "/" + sumFormula;
                column++;
            }
            excelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, startRow + 2, column - 1]);
            excelHelper.SetCustomFormat(worksheet.Cells[startRow, startColumn + 1, startRow + 2, column], "Percentage");
            bridgeWorkSummaryCommon.UpdateCurrentCell(currentCell, row, column - 1);
        }
    }
}
