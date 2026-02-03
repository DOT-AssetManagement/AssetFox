using System.Collections.Generic;
using OfficeOpenXml;
using AssetFox.Core.Reporting.Models.PAMSSummaryReport;
using AssetFox.Core.ExcelHelpers;
using System.Drawing;
using System;
using System.Linq;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Reporting.Services.PAMSSummaryReport.StaticContent;
using static AssetFox.Core.Reporting.Services.PAMSSummaryReport.PavementWorkSummary.PavementTreatmentHelper;

namespace AssetFox.Core.Reporting.Services.PAMSSummaryReport.PavementWorkSummary
{
    public class TreatmentsWorkSummary
    {
        private PavementWorkSummaryCommon _pavementWorkSummaryCommon;
        private bool ShouldBundleFeasibleTreatments;

        public TreatmentsWorkSummary()
        {
            _pavementWorkSummaryCommon = new PavementWorkSummaryCommon();
        }

        public ChartRowsModel FillTreatmentsWorkSummarySections(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>> costLengthPerSurfaceIdPerTreatmentPerYear,
            Dictionary<int, Dictionary<TreatmentGroup, (decimal treatmentCost, double length)>> costAndLengthPerTreatmentGroupPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments,
            Dictionary<TreatmentCategory, SortedDictionary<int, (decimal treatmentCost, double length)>> workTypeTotals,
            ChartRowsModel chartRowsModel,
            bool shouldBundleFeasibleTreatments)

        {
            ShouldBundleFeasibleTreatments = shouldBundleFeasibleTreatments;
            // Tables for committed projects miles
            FillCommittedTreatments(worksheet, currentCell, simulationYears, yearlyCostCommittedProj);
            FillMpmsTreatments(worksheet, currentCell, simulationYears, yearlyCostCommittedProj);
            FillSapTreatments(worksheet, currentCell, simulationYears, yearlyCostCommittedProj);
            FillProjectBuilderTreatments(worksheet, currentCell, simulationYears, yearlyCostCommittedProj);

            FillFullDepthAsphaltTreatments(worksheet, currentCell, simulationYears, costLengthPerSurfaceIdPerTreatmentPerYear, simulationTreatments);
            FillCompositeTreatments(worksheet, currentCell, simulationYears, costLengthPerSurfaceIdPerTreatmentPerYear, simulationTreatments);
            FillConcreteTreatments(worksheet, currentCell, simulationYears, costLengthPerSurfaceIdPerTreatmentPerYear, simulationTreatments);
            FillTreatmentGroups(worksheet, currentCell, simulationYears, costAndLengthPerTreatmentGroupPerYear);
                        
            FillWorkTypeTotalsSection(worksheet, currentCell, simulationYears, workTypeTotals);

            return chartRowsModel;
        }

        private void FillProjectBuilderTreatments(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj)
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Project Builder Treatments", "Project Builder Treatments");
            AddCommittedProjectsSegmentMiles(worksheet, currentCell, simulationYears, yearlyCostCommittedProj, "ProjectBuilder", PAMSConstants.ProjectBuilderTotal);
        }

        private void FillSapTreatments(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj)
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of SAP Treatments", "SAP Treatments");
            AddCommittedProjectsSegmentMiles(worksheet, currentCell, simulationYears, yearlyCostCommittedProj, "SAP", PAMSConstants.SapTotal);
        }

        private void FillMpmsTreatments(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj)
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of MPMS Treatments", "MPMS Treatments");
            AddCommittedProjectsSegmentMiles(worksheet, currentCell, simulationYears, yearlyCostCommittedProj, "MPMS", PAMSConstants.MpmsTotal);
        }        

        private void FillCommittedTreatments(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj)
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Committed Treatments", "Committed Treatments");
            AddCommittedProjectsSegmentMiles(worksheet, currentCell, simulationYears, yearlyCostCommittedProj, "Committed", PAMSConstants.CommittedTotal);
        }

        private void AddCommittedProjectsSegmentMiles(ExcelWorksheet worksheet, CurrentCell currentCell, List<int> simulationYears, Dictionary<int, Dictionary<string, List<Models.CommittedProjectMetaData>>> yearlyCostCommittedProj, string projectSource, string totalLabel)
        {            
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out var startRow, out var startColumn, out var row, out var column);
            currentCell.Column = column;
            var startYear = simulationYears[0];
            var uniqueTreatments = new Dictionary<string, int>();
            Dictionary<int, int> TotalCommittedLength = [];

            foreach (var yearlyItem in yearlyCostCommittedProj)
            {
                decimal totalLength = 0;
                row = currentCell.Row;

                foreach (var data in yearlyItem.Value)
                {
                    foreach (var committedProjectMetaData in data.Value)
                    {
                        if (committedProjectMetaData.ProjectSource == projectSource)
                        {
                            var key = data.Key.Contains("Bundle") ? data.Key : committedProjectMetaData.TreatmentCategory;
                            var cellToEnterCost = yearlyItem.Key - startYear;
                            var sectionMiles = Convert.ToDecimal(committedProjectMetaData.SectionMiles);
                            if (!uniqueTreatments.TryGetValue(key, out var value))
                            {
                                uniqueTreatments.Add(key, currentCell.Row);
                                worksheet.Cells[row++, column].Value = key;
                                worksheet.Cells[uniqueTreatments[key], column + cellToEnterCost + 2].Value = sectionMiles;
                                currentCell.Row += 1;
                            }
                            else
                            {                                
                                var currentValue = worksheet.Cells[value, column + cellToEnterCost + 2].Value;
                                decimal toAdd = currentValue == null ? 0 : Convert.ToDecimal(currentValue);
                                worksheet.Cells[value, column + cellToEnterCost + 2].Value = Convert.ToDecimal(sectionMiles + toAdd);
                            }
                            totalLength += sectionMiles;
                        }
                    }
                }
                TotalCommittedLength.Add(yearlyItem.Key, Convert.ToInt32(totalLength));
            }

            column = currentCell.Column;
            worksheet.Cells[currentCell.Row, column].Value = totalLabel;
            column++;
            int firstTotalYear = TotalCommittedLength.Count > 0 ? TotalCommittedLength.Keys.Min() : startYear;
            var offsetForTotal = firstTotalYear - startYear;
            var fromColumn = column + offsetForTotal + 1;

            foreach (var length in TotalCommittedLength)
            {
                worksheet.Cells[currentCell.Row, fromColumn++].Value = length.Value;
            }
            var committedLengthTotalRow = currentCell.Row;
            fromColumn = column + 1;
            var endColumn = simulationYears.Count + 2;

            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, endColumn]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, endColumn], Color.FromArgb(180, 198, 231)); // treatment rows
            ExcelHelper.SetCustomFormat(worksheet.Cells[startRow, fromColumn, row, endColumn], ExcelHelperCellFormat.Number);
            ExcelHelper.ApplyColor(worksheet.Cells[committedLengthTotalRow, fromColumn, committedLengthTotalRow, endColumn], Color.FromArgb(132, 151, 176)); // total row

            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, endColumn);
        }

        private void FillFullDepthAsphaltTreatments(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Full Depth Asphalt Pavement Treatments", "PAMS Full Depth Asphalt Treatments");

            var asphaltTreatments = _pavementWorkSummaryCommon.GetNoTreatments(simulationTreatments).Concat(_pavementWorkSummaryCommon.GetAsphaltTreatments(simulationTreatments)).ToList();

            AddFullDepthAsphaltTreatmentSegmentMiles(worksheet, currentCell,
                costLengthPerSurfaceIdPerTreatmentPerYear,
                asphaltTreatments
                );
        }

        private void AddFullDepthAsphaltTreatmentSegmentMiles(ExcelWorksheet worksheet, CurrentCell currentCell,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            int startRow, startColumn, row, column;
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);
            _pavementWorkSummaryCommon.SetPavementTreatmentExcelString(worksheet, simulationTreatments, ref row, ref column);
            // Bundled Treatments
            if (ShouldBundleFeasibleTreatments)
            {
                worksheet.Cells[row++, column].Value = PAMSConstants.BundledTreatments;
            }
            worksheet.Cells[row++, column].Value = PAMSConstants.AsphaltTotal;
            column++;
            var fromColumn = column + 1;
            foreach (var yearlyValues in costLengthPerSurfaceIdPerTreatmentPerYear)
            {
                row = startRow;
                column = ++column;
                double totalLength = 0;
                foreach (var treatment in simulationTreatments)
                {
                    GetLengths(yearlyValues, treatment.Name, "Asphalt", out var length);
                                        
                    totalLength += Convert.ToInt32(length);
                    worksheet.Cells[row, column].Value = Convert.ToInt32(length);
                    row++;
                }

                if (ShouldBundleFeasibleTreatments)
                {
                    double bundledLength = 0;
                    foreach (var yearlyValue in yearlyValues.Value)
                    {
                        var treatment = yearlyValue.Key;
                        if (treatment.Contains("Bundle"))
                        {
                            GetLengthsInYearlyValue(yearlyValue.Value.Where(_ => _.Key < 62).ToList(), out var length);
                            bundledLength += length;
                        }
                    }
                    totalLength += Convert.ToInt32(bundledLength);
                    worksheet.Cells[row++, column].Value = Convert.ToInt32(bundledLength);
                }

                worksheet.Cells[row, column].Value = Convert.ToInt32(totalLength);
            }
            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column], Color.FromArgb(180, 198, 231)); // treatment rows
            ExcelHelper.ApplyColor(worksheet.Cells[row, fromColumn, row, column], Color.FromArgb(132, 151, 176)); // total row

            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, column);
        }

        private void FillCompositeTreatments(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Composite Pavement Treatments", "PAMS Composite Treatments");

            var asphaltTreatments = _pavementWorkSummaryCommon.GetNoTreatments(simulationTreatments).Concat(_pavementWorkSummaryCommon.GetAsphaltTreatments(simulationTreatments)).ToList();

            AddCompositeTreatmentSegmentMiles(worksheet, currentCell,costLengthPerSurfaceIdPerTreatmentPerYear,asphaltTreatments);
        }

        private void AddCompositeTreatmentSegmentMiles(ExcelWorksheet worksheet, CurrentCell currentCell,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            int startRow, startColumn, row, column;
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);
            _pavementWorkSummaryCommon.SetPavementTreatmentExcelString(worksheet, simulationTreatments, ref row, ref column);
            // Bundled Treatments
            if (ShouldBundleFeasibleTreatments)
            {
                worksheet.Cells[row++, column].Value = PAMSConstants.BundledTreatments;
            }
            worksheet.Cells[row++, column].Value = PAMSConstants.CompositeTotal;
            column++;
            var fromColumn = column + 1;
            foreach (var yearlyValues in costLengthPerSurfaceIdPerTreatmentPerYear)
            {
                row = startRow;
                column = ++column;
                double totalLength = 0;
                foreach (var treatment in simulationTreatments)
                {
                    GetLengths(yearlyValues, treatment.Name, "Composite", out var length);
                    totalLength += Convert.ToInt32(length);
                    worksheet.Cells[row, column].Value = Convert.ToInt32(length);
                    row++;
                }

                if (ShouldBundleFeasibleTreatments)
                {
                    double bundledLength = 0;
                    foreach (var yearlyValue in yearlyValues.Value)
                    {
                        var treatment = yearlyValue.Key;
                        if (treatment.Contains("Bundle"))
                        {
                            GetLengthsInYearlyValue(yearlyValue.Value.Where(_ => _.Key == 62).ToList(), out var length);
                            bundledLength += length;
                        }
                    }
                    totalLength += Convert.ToInt32(bundledLength);
                    worksheet.Cells[row++, column].Value = Convert.ToInt32(bundledLength);
                }

                worksheet.Cells[row, column].Value = Convert.ToInt32(totalLength);
            }
            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column], Color.FromArgb(180, 198, 231));
            ExcelHelper.ApplyColor(worksheet.Cells[row, fromColumn, row, column], Color.FromArgb(132, 151, 176));
            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, column);
        }

        private void FillConcreteTreatments(
            ExcelWorksheet worksheet,
            CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            var concreteTreatments = _pavementWorkSummaryCommon.GetNoTreatments(simulationTreatments).Concat(_pavementWorkSummaryCommon.GetConcreteTreatments(simulationTreatments)).ToList();

            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Concrete Pavement Treatments", "PAMS Concrete Treatments");
            AddConcreteTreatmentSegmentMiles(worksheet, currentCell,
                costLengthPerSurfaceIdPerTreatmentPerYear,
                concreteTreatments
                );
        }

        private void AddConcreteTreatmentSegmentMiles(ExcelWorksheet worksheet, CurrentCell currentCell,
            Dictionary<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>>costLengthPerSurfaceIdPerTreatmentPerYear,
            List<(string Name, string AssetType, TreatmentCategory Category)> simulationTreatments
            )
        {
            int startRow, startColumn, row, column;
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);
            _pavementWorkSummaryCommon.SetPavementTreatmentExcelString(worksheet, simulationTreatments, ref row, ref column);
            // Bundled Treatments
            if (ShouldBundleFeasibleTreatments)
            {
                worksheet.Cells[row++, column].Value = PAMSConstants.BundledTreatments;
            }
            worksheet.Cells[row++, column].Value = PAMSConstants.ConcreteTotal;
            column++;
            var fromColumn = column + 1;
            foreach (var yearlyValues in costLengthPerSurfaceIdPerTreatmentPerYear)
            {
                row = startRow;
                column = ++column;
                double totalLength = 0;
                foreach (var treatment in simulationTreatments)
                {
                    GetLengths(yearlyValues, treatment.Name, "Concrete", out var length);
                    totalLength += Convert.ToInt32(length);
                    worksheet.Cells[row, column].Value = Convert.ToInt32(length);
                    row++;
                }

                if (ShouldBundleFeasibleTreatments)
                {
                    double bundledLength = 0;
                    foreach (var yearlyValue in yearlyValues.Value)
                    {
                        var treatment = yearlyValue.Key;
                        if (treatment.Contains("Bundle"))
                        {
                            GetLengthsInYearlyValue(yearlyValue.Value.Where(_ => _.Key > 62).ToList(), out var length);
                            bundledLength += length;
                        }
                    }
                    totalLength += Convert.ToInt32(bundledLength);
                    worksheet.Cells[row++, column].Value = Convert.ToInt32(bundledLength);
                }

                worksheet.Cells[row, column].Value = Convert.ToInt32(totalLength);
            }
            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column], Color.FromArgb(180, 198, 231));
            ExcelHelper.ApplyColor(worksheet.Cells[row, fromColumn, row, column], Color.FromArgb(132, 151, 176));

            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, column);
        }

        private void FillTreatmentGroups(ExcelWorksheet worksheet, CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<int, Dictionary<TreatmentGroup, (decimal treatmentCost, double length)>> costAndLengthPerTreatmentGroupPerYear
            )
        {
            //var workTypeConcrete = new Dictionary<TreatmentCategory, SortedDictionary<int, decimal>>();
            if (simulationYears.Count <= 0)
            {
                return;// workTypeConcrete;
            }
            var headerRange = new Range(currentCell.Row, currentCell.Row + 1);
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Section Miles of Treatment Groups", "PAMS Treatment Groups Totals");

            AddTreatmentGroupTotalDetails(worksheet, currentCell, costAndLengthPerTreatmentGroupPerYear, TreatmentGroupCategory.Bituminous);
            AddTreatmentGroupTotalDetails(worksheet, currentCell, costAndLengthPerTreatmentGroupPerYear, TreatmentGroupCategory.Concrete);
            if(ShouldBundleFeasibleTreatments)
            {
                AddTreatmentGroupTotalDetails(worksheet, currentCell, costAndLengthPerTreatmentGroupPerYear, TreatmentGroupCategory.Bundled);
            }
        }

        private void AddTreatmentGroupTotalDetails(ExcelWorksheet worksheet, CurrentCell currentCell,
            Dictionary<int, Dictionary<TreatmentGroup, (decimal treatmentCost, double length)>> costAndLengthPerTreatmentGroupPerYear,
            TreatmentGroupCategory treatmentGroupCategory)
        {
            int startRow, startColumn, row, column;
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);

            var treatmentGroups = GetListOfTreatmentGroupForCategory(treatmentGroupCategory);

            var prefix = GetTreatmentGroupString(treatmentGroupCategory) + " - ";
            var treatmentGroupTitles = treatmentGroups.Select(tg => prefix + tg.GroupDescription).Distinct().ToList();

            _pavementWorkSummaryCommon.SetPavementTreatmentGroupsExcelString(worksheet, treatmentGroupTitles, ref row, ref column);

            column++;
            var fromColumn = column + 1;
            var descriptions = treatmentGroups.Select(_ => _.GroupDescription).Distinct().ToList();
            foreach (var yearlyValues in costAndLengthPerTreatmentGroupPerYear)
            {
                row = startRow;
                column = ++column;
                foreach (var description in descriptions)
                {
                    double treatmentLength = 0;
                    foreach (var treatmentGroup in treatmentGroups.Where(_ => _.GroupDescription.Equals(description)))
                    {
                        yearlyValues.Value.TryGetValue(treatmentGroup, out var costAndLength);
                        treatmentLength += Convert.ToInt32(costAndLength.length);
                    }
                    worksheet.Cells[row, column].Value = Convert.ToInt32(treatmentLength);
                    row++;
                }
            }
            row--;

            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column], Color.FromArgb(180, 198, 231));

            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, ++row, column);
        }

        private Dictionary<TreatmentCategory, SortedDictionary<int, decimal>> FillWorkTypeTotalsSection(ExcelWorksheet worksheet, CurrentCell currentCell,
            List<int> simulationYears,
            Dictionary<TreatmentCategory, SortedDictionary<int, (decimal treatmentCost, double length)>> workTypeTotals
            )
        {
            var workTypesForReport = new List<TreatmentCategory> { TreatmentCategory.Maintenance, TreatmentCategory.Preservation, TreatmentCategory.Rehabilitation, TreatmentCategory.Reconstruction };
            var headerRange = new Range(currentCell.Row, currentCell.Row + 1);
            _pavementWorkSummaryCommon.AddHeaders(worksheet, currentCell, simulationYears, "Total Number of Section Miles", "Work Type Totals");

            int startRow, startColumn, row, column;
            _pavementWorkSummaryCommon.SetRowColumns(currentCell, out startRow, out startColumn, out row, out column);

            var workTypeTitles = workTypesForReport.Select(tc => tc.ToSpreadsheetString()).ToList();
            workTypeTitles.Add("Total");

            _pavementWorkSummaryCommon.SetPavementTreatmentGroupsExcelString(worksheet, workTypeTitles, ref row, ref column);

            column++;
            var fromColumn = column + 1;

            row = startRow;

            var columnTotals = new Dictionary<int, double>();

            foreach (var workType in workTypesForReport)
            {
                column = fromColumn;

                var workTypeTotalExists = workTypeTotals.TryGetValue(workType, out var workTypeTotal);

                foreach (var year in simulationYears)
                {
                    if (!columnTotals.ContainsKey(year))
                    {
                        columnTotals.Add(year, 0);
                    }
                    if (workTypeTotalExists && (workTypeTotal.TryGetValue(year, out var costAndLength)))
                    {
                        worksheet.Cells[row, column].Value = Convert.ToInt32(costAndLength.length);
                        columnTotals[year] += Convert.ToInt32(costAndLength.length);
                    }
                    else
                    {
                        worksheet.Cells[row, column].Value = 0.0;
                    }
                    column++;
                }

                row++;
            }

            // Add Total Row
            column = fromColumn;
            foreach (var year in simulationYears)
            {
                worksheet.Cells[row, column].Value = columnTotals[year];
                column++;
            }

            column = fromColumn + simulationYears.Count;

            ExcelHelper.ApplyBorder(worksheet.Cells[startRow, startColumn, row, column - 1]);
            ExcelHelper.ApplyColor(worksheet.Cells[startRow, fromColumn, row, column - 1], Color.FromArgb(132, 151, 176));

            ExcelHelper.ApplyColor(worksheet.Cells[row + 2, startColumn, row + 2, column - 1], Color.FromArgb(89, 89, 89));

            _pavementWorkSummaryCommon.UpdateCurrentCell(currentCell, row + 4, column);

            return null;
        }

        private static void GetLengths(KeyValuePair<int, Dictionary<string, Dictionary<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>>> yearlyValues, string treatment, string type, out double length)
        {
            length = 0;
            _ = yearlyValues.Value.TryGetValue(treatment, out var costAndLengthsPerSurfaceId);            
            if (costAndLengthsPerSurfaceId != null)
            {
                foreach (var value in costAndLengthsPerSurfaceId)
                {
                    switch (type)
                    {
                    case "Asphalt": if (value.Key < 62) { length += value.Value.length; } break;
                    case "Composite": if (value.Key == 62) { length += value.Value.length; } break;
                    case "Concrete": if (value.Key > 62) { length += value.Value.length; } break;
                    };
                }
            }
        }

        private static void GetLengthsInYearlyValue(List<KeyValuePair<int, (decimal treatmentCost, decimal compositeTreatmentCost, double length)>> valuesPerSurfaceId, out double length)
        {
            length = 0;
            foreach (var value in valuesPerSurfaceId)
            {
                length += value.Value.length;
            }
        }
    };
}
