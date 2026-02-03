// AssetFox.Core.Reporting/Services/PAMSSummaryReport/SectionSummary/SectionSummary.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models.PAMSSummaryReport;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.PAMSSummaryReport.SectionSummary
{
    public class SectionSummary
    {
        private readonly SummaryReportHelper _summaryReportHelper = new SummaryReportHelper();

        public void Fill(ExcelWorksheet worksheet, SimulationOutput reportOutputData, List<int> simulationYears, Dictionary<string, string> treatmentCategoryLookup, bool shouldBundleFeasibleTreatments, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, string primaryKey)
        {
            BuildHeaders(worksheet, simulationYears);
            FillData(worksheet, reportOutputData, simulationYears, treatmentCategoryLookup, shouldBundleFeasibleTreatments, keyCashFlowFundingDetails, primaryKey);
            worksheet.Cells.AutoFitColumns();
        }

        private void BuildHeaders(ExcelWorksheet worksheet, List<int> simulationYears)
        {
            worksheet.Cells[1, 1].Value = "CRS";
            ExcelHelper.MergeCells(worksheet, 1, 1, 2, 1);

            var col = 2;
            foreach (var year in simulationYears)
            {
                worksheet.Cells[1, col].Value = year;
                ExcelHelper.MergeCells(worksheet, 1, col, 1, col + 2);
                worksheet.Cells[2, col].Value = "Treatment(s)";
                worksheet.Cells[2, col + 1].Value = "Total Cost";
                worksheet.Cells[2, col + 2].Value = "Predominant Category";
                col += 3;
            }
            ExcelHelper.ApplyStyle(worksheet.Cells[1, 1, 2, col - 1]);
        }

        private void FillData(ExcelWorksheet worksheet, SimulationOutput reportOutputData, List<int> simulationYears, Dictionary<string, string> treatmentCategoryLookup, bool shouldBundleFeasibleTreatments, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, string primaryKey)
        {
            var crsValues = reportOutputData.InitialAssetSummaries
                .Select(s => _summaryReportHelper.checkAndGetValue(s.ValuePerTextAttribute, "CRS"))
                .Distinct()
                .OrderBy(crs => crs)
                .ToList();

            var row = 3;
            foreach (var crs in crsValues)
            { 
                worksheet.Cells[row, 1].Value = crs;
                var col = 2;
                foreach (var year in simulationYears)
                {
                    var assetsInCrsAndYear = reportOutputData.Years
                        .First(y => y.Year == year).Assets
                        .Where(a => _summaryReportHelper.checkAndGetValue(a.ValuePerTextAttribute, "CRS") == crs)
                        .ToList();

                    var treatments = new List<string>();
                    decimal totalCost = 0;
                    var categories = new List<string>();

                    foreach (var asset in assetsInCrsAndYear)
                    {
                        var primaryKeyValue = _summaryReportHelper.checkAndGetValue(asset.ValuePerTextAttribute, primaryKey);
                        var treatmentConsiderations = ((asset.TreatmentCause == TreatmentCause.SelectedTreatment &&
                                                      asset.TreatmentStatus == TreatmentStatus.Progressed) ||
                                                      (asset.TreatmentCause == TreatmentCause.CashFlowProject &&
                                                      asset.TreatmentStatus == TreatmentStatus.Progressed) ||
                                                      (asset.TreatmentCause == TreatmentCause.CashFlowProject &&
                                                      asset.TreatmentStatus == TreatmentStatus.Applied)) &&
                                                      keyCashFlowFundingDetails.ContainsKey(primaryKeyValue) ?
                                                      keyCashFlowFundingDetails[primaryKeyValue] :
                                                      asset.TreatmentConsiderations ?? new();

                        var treatmentConsideration = shouldBundleFeasibleTreatments ?
                                             treatmentConsiderations.FirstOrDefault(_ => _.FundingCalculationOutput != null &&
                                                _.FundingCalculationOutput.AllocationMatrix.Any(a => a.Year == year) &&
                                                asset.AppliedTreatment.Contains(_.TreatmentName)) :
                                             treatmentConsiderations.FirstOrDefault(_ => _.FundingCalculationOutput != null &&
                                                _.FundingCalculationOutput.AllocationMatrix.Any(a => a.Year == year) &&
                                                _.TreatmentName == asset.AppliedTreatment);

                        var appliedTreatment = treatmentConsideration?.TreatmentName ?? asset.AppliedTreatment;

                        if (appliedTreatment.ToLower() != "no treatment")
                        {
                            ExtractTreatments(appliedTreatment, treatments);
                            var cost = treatmentConsideration?.FundingCalculationOutput?.AllocationMatrix?.
                               Where(_ => _.Year == year).
                               Sum(b => b.AllocatedAmount) ?? 0;
                            totalCost += cost;

                            var individualTreatments = new List<string>();
                            ExtractTreatments(appliedTreatment, individualTreatments);
                            foreach (var treat in individualTreatments)
                            {
                                if (treatmentCategoryLookup.ContainsKey(treat))
                                {
                                    categories.Add(treatmentCategoryLookup[treat]);
                                }
                            }
                        }
                    }

                    var distinctTreatments = treatments.Distinct().ToList();
                    string treatmentDisplay;
                    switch (distinctTreatments.Count)
                    {
                        case 0:
                            treatmentDisplay = "--";
                            break;
                        case 1:
                            treatmentDisplay = distinctTreatments.First();
                            break;
                        default:
                            treatmentDisplay = $"Bundle[{string.Join("|", distinctTreatments)}]";
                            break;
                    }
                    worksheet.Cells[row, col].Value = treatmentDisplay;
                    worksheet.Cells[row, col + 1].Value = totalCost;
                    worksheet.Cells[row, col + 1].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, col + 2].Value = GetPredominantCategory(categories);

                    col += 3;
                }

                if (row % 2 != 0)
                {
                    ExcelHelper.ApplyColor(worksheet.Cells[row, 1, row, col - 1], Color.LightGray);
                }

                row++;
            }
        }

        private void ExtractTreatments(string treatmentString, List<string> treatments)
        {
            if (treatmentString.StartsWith("Bundle", StringComparison.OrdinalIgnoreCase))
            {
                var matches = Regex.Matches(treatmentString, @"\[(.*?)\]");
                foreach (Match match in matches)
                {
                    var content = match.Groups[1].Value;
                    treatments.AddRange(content.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
                }
            }
            else
            {
                treatments.Add(treatmentString);
            }
        }

        private string GetPredominantCategory(List<string> categories)
        {
            if (!categories.Any())
            {
                return "N/A";
            }

            return categories.GroupBy(c => c)
                             .OrderByDescending(g => g.Count())
                             .First()
                             .Key;
        }
    }
}
