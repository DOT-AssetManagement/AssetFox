using System;
using System.Linq;
using System.Collections.Generic;

using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.DistrictCountyTotals
{
    public static class DistrictTotalsExcelModels
    {
        private static decimal TotalCost(AssetDetail section, int year)
            => section.TreatmentConsiderations.Sum(_ => _.FundingCalculationOutput?.AllocationMatrix.Where(_ => _.Year == year).Sum(b => b.AllocatedAmount) ?? 0);

        internal static IExcelModel DistrictTableContent(
            SimulationYearDetail year,
            List<AssetSummaryDetail> initialAssetSummaries,
            Func<AssetDetail, AssetSummaryDetail, bool> inclusionPredicate)
        {
            var totalMoney = DistrictTableContentValue(year, initialAssetSummaries, inclusionPredicate);
            return StackedExcelModels.Stacked(
                ExcelValueModels.Money(totalMoney),
                ExcelStyleModels.Right,
                ExcelStyleModels.ThinBorder,
                ExcelStyleModels.CurrencyWithoutCentsFormat,
                DistrictTotalsStyleModels.LightGreenFill
                );
        }

        internal static decimal DistrictTableContentValue(
            SimulationYearDetail year,
            List<AssetSummaryDetail> initialAssetSummaries,
            Func<AssetDetail, AssetSummaryDetail, bool> inclusionPredicate)
        {
            decimal totalMoney = 0;
            var sections = year.Assets;
            foreach (var section in sections)
            {
                try
                {
                    if (inclusionPredicate(section, initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId)))
                    {
                        var cost = TotalCost(section, year.Year);
                        totalMoney += cost;
                    }
                }
                catch
                {
                    // just swallow the error and skip the section
                }
            }
            return totalMoney;
        }
    }
}
