using System;
using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models.BAMSSummaryReport;
using AssetFox.Core.Reporting.Services.BAMSSummaryReport.BridgeWorkSummary;
using AssetFox.Core.Reporting.Models;
using System.Linq;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.BridgeWorkSummaryByBudget
{
    public class CommittedProjectCost
    {
        private readonly BridgeWorkSummaryCommon _bridgeWorkSummaryCommon;

        public CommittedProjectCost()
        {
            _bridgeWorkSummaryCommon = new BridgeWorkSummaryCommon();
        }

        private void FillCostSectionByWorkType(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            List<YearsData> costData,
            string projectSource,
            string totalLabel,
            WorkTypeTotal workTypeTotal)
        {
            var startYear = simulationYears[0];
            var startOfSection = currentCell.Row;

            // 1. Aggregate costs by Work Type and Year
            var costsByWorkType = new Dictionary<string, Dictionary<int, double>>();
            foreach (var item in costData.Where(i => i.ProjectSource == projectSource))
            {
                // Use the TreatmentCategory (Work Type) as the key for grouping
                var workType = item.TreatmentCategory.ToSpreadsheetString();
                if (string.IsNullOrEmpty(workType))
                {
                    workType = "Other"; // Fallback for safety
                }

                if (!costsByWorkType.ContainsKey(workType))
                {
                    costsByWorkType[workType] = new Dictionary<int, double>();
                }
                if (!costsByWorkType[workType].ContainsKey(item.Year))
                {
                    costsByWorkType[workType][item.Year] = 0.0;
                }
                costsByWorkType[workType][item.Year] += item.Amount;

                // This helper correctly populates the overall WorkTypeTotal for the summary section later
                WorkTypeTotalHelper.FillWorkTypeTotals(item, workTypeTotal);
            }

            // 2. Write aggregated data to the worksheet, one row per Work Type
            foreach (var workType in costsByWorkType.Keys.OrderBy(k => k))
            {
                var rowNum = currentCell.Row++;
                worksheet.Cells[rowNum, currentCell.Column].Value = workType;
                worksheet.Cells[rowNum, currentCell.Column + 2, rowNum, currentCell.Column + 1 + simulationYears.Count].Value = 0.0;

                foreach (var yearlyCost in costsByWorkType[workType])
                {
                    var year = yearlyCost.Key;
                    var cost = yearlyCost.Value;
                    var cellToEnterCost = year - startYear;
                    worksheet.Cells[rowNum, currentCell.Column + cellToEnterCost + 2].Value = cost;
                }
            }

            worksheet.Cells[currentCell.Row, currentCell.Column].Value = totalLabel;

            // 3. Calculate and write the total row
            var totalBudgetPerYear = costData
                .Where(item => item.ProjectSource == projectSource)
                .GroupBy(item => item.Year)
                .ToDictionary(g => g.Key, g => g.Sum(item => (decimal)item.Amount));

            foreach (var totalBudget in totalBudgetPerYear)
            {
                var cellToEnterTotalCost = totalBudget.Key - startYear;
                worksheet.Cells[currentCell.Row, currentCell.Column + cellToEnterTotalCost + 2].Value = totalBudget.Value;
            }

            // Apply formatting
            ExcelHelper.ApplyBorder(worksheet.Cells[startOfSection, currentCell.Column, currentCell.Row, simulationYears.Count + 2]);
            ExcelHelper.SetCustomFormat(worksheet.Cells[startOfSection, currentCell.Column + 2, currentCell.Row, simulationYears.Count + 2], ExcelHelperCellFormat.NegativeCurrency);
            ExcelHelper.ApplyColor(worksheet.Cells[startOfSection, currentCell.Column + 2, currentCell.Row, simulationYears.Count + 2], Color.DarkSeaGreen);
            ExcelHelper.ApplyColor(worksheet.Cells[currentCell.Row, currentCell.Column + 2, currentCell.Row, simulationYears.Count + 2], Color.FromArgb(84, 130, 53));
            ExcelHelper.SetTextColor(worksheet.Cells[currentCell.Row, currentCell.Column + 2, currentCell.Row, simulationYears.Count + 2], Color.White);
            currentCell.Row++;
        }

        internal void FillCostOfCommittedWork(ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            List<YearsData> costForCommittedBudgets,
            Dictionary<int, decimal> totalBudgetPerYearForCommittedWork,
            WorkTypeTotal workTypeTotal)
        {
            currentCell.Row += 1;
            _bridgeWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Cost of Committed Work", "Committed Work Type");
            currentCell.Row += 1;
            currentCell.Column = 1;
            FillCostSectionByWorkType(worksheet, currentCell, simulationYears, costForCommittedBudgets, "Committed", BAMSConstants.CommittedTotal, workTypeTotal);
        }

        internal void FillCostOfMPMSWork(ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            List<YearsData> costForCommittedBudgets,
            Dictionary<int, decimal> totalBudgetPerYearForCommittedWork,
            WorkTypeTotal workTypeTotal)
        {
            currentCell.Row += 1;
            _bridgeWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Cost of MPMS Work", "MPMS Work Type");
            currentCell.Row += 1;
            currentCell.Column = 1;
            FillCostSectionByWorkType(worksheet, currentCell, simulationYears, costForCommittedBudgets, "MPMS", BAMSConstants.CommittedTotal, workTypeTotal);
        }

        internal void FillCostOfSAPWork(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            List<YearsData> costForSAPBudgets,
            Dictionary<int, decimal> totalBudgetPerYearForSAPWork,
            WorkTypeTotal workTypeTotal)
        {
            currentCell.Row += 1;
            _bridgeWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Cost of SAP Work", "SAP Work Type");
            currentCell.Row += 1;
            currentCell.Column = 1;
            FillCostSectionByWorkType(worksheet, currentCell, simulationYears, costForSAPBudgets, "SAP", BAMSConstants.SAPTotal, workTypeTotal);
        }

        internal void FillCostOfProjectBuilderWork(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            List<YearsData> costForProjectBuilderBudgets,
            Dictionary<int, decimal> totalBudgetPerYearForProjectBuilderWork,
            WorkTypeTotal workTypeTotal)
        {
            currentCell.Row += 1;
            _bridgeWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Cost of Project Builder Work", "Project Builder Work Type");
            currentCell.Row += 1;
            currentCell.Column = 1;
            FillCostSectionByWorkType(worksheet, currentCell, simulationYears, costForProjectBuilderBudgets, "ProjectBuilder", BAMSConstants.ProjectBuilderTotal, workTypeTotal);
            currentCell.Row++; // Add extra space after the section
        }

        internal void AddCostOfWorkOutsideScope(WorkTypeTotal workTypeTotal, List<BaseCommittedProjectDTO> committedProjectsForWorkOutsideScope, Guid? scenarioBudgetId)
        {
            var committedProjectsForWorkOutsideScopeFiltered = committedProjectsForWorkOutsideScope.Where(_ => _.ScenarioBudgetId == scenarioBudgetId);
            foreach (var committedProjectForWorkOutsideScope in committedProjectsForWorkOutsideScopeFiltered)
            {
                var yearsData = new YearsData { TreatmentCategory = TreatmentCategory.WorkOutsideScope, Year = committedProjectForWorkOutsideScope.Year, Amount = committedProjectForWorkOutsideScope.Cost };
                WorkTypeTotalHelper.FillWorkTypeTotals(yearsData, workTypeTotal);
            }
        }
    }
}
