using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;

namespace AssetFox.Core.Reporting.Services.BAMSSummaryReport.BridgeWorkSummary
{
    public class BridgeWorkSummaryComputationHelper
    {
        private ReportHelper _reportHelper;
        private readonly IUnitOfWork _unitOfWork;

        public BridgeWorkSummaryComputationHelper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        public int TotalInitialPoorBridgesCount(SimulationOutput reportOutputData) => CountAndAreaOfBridges.GetTotalInitialPoorCount(reportOutputData.InitialAssetSummaries);

        public int TotalSectionalPoorBridgesCount(SimulationYearDetail YearlyData) => CountAndAreaOfBridges.GetTotalSectionalPoorCount(YearlyData.Assets);

        public double TotalInitialPoorBridgesDeckArea(SimulationOutput reportOutputData) => CountAndAreaOfBridges.GetTotalInitialPoorDeckArea(reportOutputData.InitialAssetSummaries);

        public double CalculateTotalPoorBridgesDeckArea(SimulationYearDetail yearlyData, List<AssetSummaryDetail> initialAssetSummaries) => CountAndAreaOfBridges.TotalPoorBridgesDeckArea(yearlyData.Assets, initialAssetSummaries);

        internal int TotalInitialBridgeGoodCount(SimulationOutput reportOutputData) => reportOutputData.InitialAssetSummaries.Where(_ => ConditionIsGood(_)).Count();

        internal int CalculateTotalBridgeGoodCount(SimulationYearDetail yearlyData) => yearlyData.Assets.Where(_ => ConditionIsGood(_)).Count();

        internal int TotalInitialBridgeClosedCount(SimulationOutput reportOutputData) => reportOutputData.InitialAssetSummaries.Where(_ => IsClosed(_)).Count();

        internal int CalculateTotalBridgeClosedCount(SimulationYearDetail yearlyData) => yearlyData.Assets.Where(_ => IsClosed(_)).Count();

        internal double CalculatePoorCountOrAreaForBPN(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridgesInitial = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var filteredSections = FilterSections(sectionDetails, postedBridgesInitial);
            var selectedBridges = filteredSections.FindAll(ConditionIsPoor);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, selectedBridges);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculatePoorCountOrAreaForBPN(List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridges = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var selectedBridges = postedBridges.FindAll(section => ConditionIsPoor(section));
            if (isCount)
            {
                return selectedBridges.Count;
            }
            return selectedBridges.Sum(_ => _.ValuePerNumericAttribute["DECK_AREA"]);
        }


        internal List<AssetDetail> GetCashflowChainLeft(AssetDetail asset, SimulationOutput simulationOutput, SimulationYearDetail currentYearDetail, string primaryKey)
        {
            // return cash flow chain preceding this section (i.e. related w/ TreatmentCause == TreatmentCause.CashFlowProject or in previous consecutive years)

            var id = Convert.ToInt32(_reportHelper.CheckAndGetValue(asset.ValuePerNumericAttribute, primaryKey));
            var chainStack = new Stack<AssetDetail>();

            var assetIndex = simulationOutput.Years.IndexOf(currentYearDetail);
            if (assetIndex > 0)
            {
                var done = false;
                for (var currentIndex = assetIndex - 1; currentIndex > 0 && !done; currentIndex--)
                {
                    var currentYear = simulationOutput.Years[currentIndex];
                    var currentAsset = currentYear.Assets.Single(chainAsset => chainAsset.AppliedTreatment == asset.AppliedTreatment && Convert.ToInt32(_reportHelper.CheckAndGetValue(chainAsset.ValuePerNumericAttribute, primaryKey)) == id);
                    done = currentAsset.TreatmentCause == TreatmentCause.SelectedTreatment;
                    chainStack.Push(currentAsset);
                }
            }

            var chain = chainStack.ToList();
            return chain;
        }

        internal List<AssetDetail> GetCashflowChainRight(AssetDetail asset, SimulationOutput simulationOutput, SimulationYearDetail currentYearDetail, string primaryKey)
        {
            // return cash flow chain following this section (i.e. related w/ TreatmentCause == TreatmentCause.CashFlowProject or TreatmentCause.SelectedTreatment in consecutive years; TreatmentCause.SelectedTreatment is the root)
            var id = Convert.ToInt32(_reportHelper.CheckAndGetValue(asset.ValuePerNumericAttribute, primaryKey));
            var chain = new List<AssetDetail>();

            var assetIndex = simulationOutput.Years.IndexOf(currentYearDetail);
            if (assetIndex > -1)
            {
                var done = false;
                for (var currentIndex = assetIndex + 1; currentIndex < (simulationOutput.Years.Count) && !done; currentIndex++)
                {
                    var currentYear = simulationOutput.Years[currentIndex];
                    var currentAsset = currentYear.Assets.FirstOrDefault(chainAsset => chainAsset.AppliedTreatment == asset.AppliedTreatment && Convert.ToInt32(_reportHelper.CheckAndGetValue(chainAsset.ValuePerNumericAttribute, primaryKey)) == id);
                    if (currentAsset != null)
                    {
                        chain.Add(currentAsset);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }

            return chain;
        }

        internal List<AssetDetail> GetCashflowChain(AssetDetail section, SimulationOutput simulationOutput, SimulationYearDetail currentYearDetail, string primaryKey)
        {
            var chain = new List<AssetDetail>();
            if (section.TreatmentCause == TreatmentCause.CashFlowProject)
            {
                chain.AddRange(GetCashflowChainLeft(section, simulationOutput, currentYearDetail, primaryKey));
                chain.Add(section);
                chain.AddRange(GetCashflowChainRight(section, simulationOutput, currentYearDetail, primaryKey));
            }
            else
            {
                chain.Add(section);
                chain.AddRange(GetCashflowChainRight(section, simulationOutput, currentYearDetail, primaryKey));
            }
            return chain;
        }

        internal double CashFlowChainSectionCost(List<AssetDetail> sectionDetails)
        {
            var initial = sectionDetails.First();
            var firstOption = initial.TreatmentOptions.FirstOrDefault(t => t.TreatmentName == initial.AppliedTreatment);
            if (firstOption == null)
            {
                return 0;
            }
            else
            {
                return firstOption.Cost / sectionDetails.Count;
            }
        }

        internal double CalculateMoneyNeededByBPN(List<AssetDetail> sectionDetails, string bpn, SimulationOutput simulationOutput, SimulationYearDetail currentYearDetail, string primaryKey)
        {
            var filteredinitialAssetSummaries = simulationOutput.InitialAssetSummaries.Where(_ => _reportHelper.CheckAndGetValue(_.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var filteredinitialAssetSummariesAssetIds = filteredinitialAssetSummaries.Select(_=>_.AssetId).ToList();
            var filteredBPNBridges = sectionDetails.FindAll(b =>
                b.TreatmentCause != TreatmentCause.NoSelection &&
                b.TreatmentCause != TreatmentCause.SelectedTreatment &&
                b.TreatmentCause != TreatmentCause.CashFlowProject &&
                b.TreatmentOptions.Count > 0 &&
                filteredinitialAssetSummariesAssetIds.Contains(b.AssetId));

            var totalCost = filteredBPNBridges.Sum(_ => _.TreatmentOptions.FirstOrDefault(t => t.TreatmentName == _.AppliedTreatment).Cost);

            var cashFlowBPNBridges = sectionDetails.FindAll(b =>
                (b.TreatmentCause == TreatmentCause.CashFlowProject || b.TreatmentCause == TreatmentCause.SelectedTreatment &&
                b.TreatmentOptions.Count > 0) &&
                filteredinitialAssetSummariesAssetIds.Contains(b.AssetId));
            var cashFlowChainsTotal = cashFlowBPNBridges.Select(section => GetCashflowChain(section, simulationOutput, currentYearDetail, primaryKey)).Sum(_ => CashFlowChainSectionCost(_));

            totalCost += cashFlowChainsTotal;

            var committedBPNBridges = sectionDetails.FindAll(b =>
                b.TreatmentCause == TreatmentCause.CommittedProject &&
                filteredinitialAssetSummariesAssetIds.Contains(b.AssetId));

            foreach (var section in committedBPNBridges)
            {
                var cost = section.TreatmentConsiderations?
                    .SelectMany(tc => tc.FundingCalculationOutput?.AllocationMatrix ?? new())
                    .Where(a => a.Year == currentYearDetail.Year)
                    .Sum(a => (double)a.AllocatedAmount) ?? 0;
                totalCost += cost;
            }

            return totalCost;
        }


        internal int CalculateTotalBridgePoorCount(SimulationYearDetail yearlyData)
        {
            var poorCount = 0;
            foreach (var section in yearlyData.Assets)
            {
                poorCount += ConditionIsPoor(section) ? 1 : 0;
            }
            return poorCount;
        }

        internal double TotalInitialGoodDeckArea(SimulationOutput reportOutputData)
        {
            double sum = 0;
            foreach (var initialSection in reportOutputData.InitialAssetSummaries)
            {
                var area = ConditionIsGood(initialSection) ? initialSection.ValuePerNumericAttribute["DECK_AREA"] : 0;
                sum += area;
            }

            return sum;
        }

        internal double TotalInitialPoorDeckArea(SimulationOutput reportOutputData) => CountAndAreaOfBridges.GetTotalInitialPoorDeckArea(reportOutputData.InitialAssetSummaries);

        internal double InitialTotalDeckArea(SimulationOutput reportOutputData)
        {
            double sum = 0;
            foreach (var initialSection in reportOutputData.InitialAssetSummaries)
            {
                sum += _reportHelper.CheckAndGetValue(initialSection.ValuePerNumericAttribute, "DECK_AREA");
            }

            return sum;
        }


        internal double TotalInitialClosedDeckArea(SimulationOutput reportOutputData)
        {
            double sum = 0;
            foreach (var initialSection in reportOutputData.InitialAssetSummaries)
            {
                var area = IsClosed(initialSection) ? _reportHelper.CheckAndGetValue(initialSection.ValuePerNumericAttribute, "DECK_AREA") : 0;
                sum += area;
            }

            return sum;
        }

        internal double CalculateTotalGoodDeckArea(SimulationYearDetail yearlyData, List<AssetSummaryDetail> initialAssetSummaries)
        {
            double sum = 0;
            foreach (var section in yearlyData.Assets)
            {
                var initialAssetSummary = initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId);
                var area = ConditionIsGood(section) ? _reportHelper.CheckAndGetValue(initialAssetSummary.ValuePerNumericAttribute, "DECK_AREA") : 0;
                sum += area;
            }

            return sum;
        }

        internal double CalculateTotalClosedDeckArea(SimulationYearDetail yearlyData, List<AssetSummaryDetail> initialAssetSummaries)
        {
            double sum = 0;
            foreach (var section in yearlyData.Assets)
            {
                var initialAssetSummary = initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId);
                var area = IsClosed(section) ? _reportHelper.CheckAndGetValue(initialAssetSummary.ValuePerNumericAttribute, "DECK_AREA") : 0;
                sum += area;
            }

            return sum;
        }

        internal double SectionalNHSBridgeGoodCountOrArea(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            var filteredSections = FilterSections(sectionDetails, filteredInitialSections);
            var resultSections = filteredSections.FindAll(ConditionIsGood);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, resultSections);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        private static List<AssetSummaryDetail> FilterInitialSections(List<AssetSummaryDetail> initialAssetSummaries, List<AssetDetail> sections)
        {
            var filteredSectionsAssetIds = sections.Select(_ => _.AssetId).ToList();
            var goodInitialSections = initialAssetSummaries.Where(_ => filteredSectionsAssetIds.Contains(_.AssetId)).ToList();

            return goodInitialSections;
        }

        private static List<AssetDetail> FilterSections(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> filteredInitialSections)
        {
            var filteredInitialSectionsAssetIds = filteredInitialSections.Select(_ => _.AssetId).ToList();
            var filteredSections = sectionDetails.Where(_ => filteredInitialSectionsAssetIds.Contains(_.AssetId)).ToList();

            return filteredSections;
        }

        private List<AssetSummaryDetail> FilterInitialSectionsForNhsInd(List<AssetSummaryDetail> initialAssetSummaries) =>
            initialAssetSummaries.FindAll(_ => int.TryParse(_reportHelper.CheckAndGetValue(_.ValuePerTextAttribute, "NHS_IND"), out var numericValue)
            && numericValue > 0);

        internal double TotalNHSBridgeCountOrArea(List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            return isCount
                ? filteredInitialSections.Count
                : filteredInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double SectionalNHSBridgePoorCountOrArea(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            var filteredSections = FilterSections(sectionDetails, filteredInitialSections);
            var resultSections = filteredSections.FindAll(ConditionIsPoor);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, resultSections);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double SectionalNHSBridgeClosedCountOrArea(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            var filteredSections = FilterSections(sectionDetails, filteredInitialSections);
            var resultSections = filteredSections.FindAll(IsClosed);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, resultSections);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double InitialNHSBridgePoorCountOrArea(List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            var poorSections = filteredInitialSections.FindAll(ConditionIsPoor);
            return isCount
                ? poorSections.Count
                : poorSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double InitialNHSBridgeClosedCountOrArea(List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredInitialSections = FilterInitialSectionsForNhsInd(initialAssetSummaries);
            var closedSections = filteredInitialSections.FindAll(IsClosed);
            return isCount
                ? closedSections.Count
                : closedSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double InitialNHSBridgeGoodCountOrArea(List<AssetSummaryDetail> initialAssetSummaries, bool isCount)
        {
            var filteredSection = initialAssetSummaries.FindAll(_ => int.TryParse(_reportHelper.CheckAndGetValue(_.ValuePerTextAttribute, "NHS_IND"), out var numericValue)
            && numericValue > 0);
            var goodSections = filteredSection.FindAll(s => ConditionIsGood(s));
            if (isCount)
            {
                return goodSections.Count;
            }
            return goodSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculateTotalPoorDeckArea(SimulationYearDetail yearlyData, List<AssetSummaryDetail> initialAssetSummaries)
        {
            double sum = 0;
            foreach (var section in yearlyData.Assets)
            {
                var initialAssetSummary = initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId);
                var area = ConditionIsPoor(section) ? _reportHelper.CheckAndGetValue(initialAssetSummary.ValuePerNumericAttribute, "DECK_AREA") : 0;
                sum += area;
            }

            return sum;
        }

        internal double CalculateTotalDeckArea(SimulationYearDetail yearlyData, List<AssetSummaryDetail> initialAssetSummaries)
        {
            double sum = 0;
            foreach (var section in yearlyData.Assets)
            {
                var initialAssetSummary = initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId);
                sum += _reportHelper.CheckAndGetValue(initialAssetSummary.ValuePerNumericAttribute, "DECK_AREA");
            }

            return sum;
        }

        internal double CalculateClosedCountOrDeckAreaForBPN(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridgesInitial = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var filteredSections = FilterSections(sectionDetails, postedBridgesInitial);
            var selectedBridges = filteredSections.FindAll(IsClosed);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, selectedBridges);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculateClosedCountOrDeckAreaForBPN(List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridges = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var selectedBridges = postedBridges.FindAll(section => IsClosed(section));
            if (isCount)
            {
                return selectedBridges.Count;
            }
            return selectedBridges.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculatePostedCountOrDeckAreaForBPN(List<AssetDetail> sectionDetails, List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridgesInitial = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var filteredSections = FilterSections(sectionDetails, postedBridgesInitial);
            var selectedBridges = filteredSections.FindAll(IsPosted);
            var resultInitialSections = FilterInitialSections(initialAssetSummaries, selectedBridges);
            return isCount
                ? resultInitialSections.Count
                : resultInitialSections.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculatePostedCountOrDeckAreaForBPN(List<AssetSummaryDetail> initialAssetSummaries, string bpn, bool isCount)
        {
            var postedBridges = initialAssetSummaries.FindAll(b => _reportHelper.CheckAndGetValue(b.ValuePerTextAttribute, "BUS_PLAN_NETWORK") == bpn);
            var selectedBridges = postedBridges.FindAll(section => IsPosted(section));
            if (isCount)
            {
                return selectedBridges.Count;
            }
            return selectedBridges.Sum(_ => _reportHelper.CheckAndGetValue(_.ValuePerNumericAttribute, "DECK_AREA"));
        }

        internal double CalculatePostedCount(List<AssetDetail> sectionDetails) => sectionDetails.Count(_ => IsPosted(_));
        internal double CalculatePostedCount(List<AssetSummaryDetail> initialAssetSummaries) => initialAssetSummaries.Count(section => IsPosted(section));
        internal double CalculateClosedCount(List<AssetDetail> sectionDetails) => sectionDetails.Count(_ => IsClosed(_));
        internal double CalculateClosedCount(List<AssetSummaryDetail> initialAssetSummaries) => initialAssetSummaries.Count(_ => IsClosed(_));

        #region Private methods

        internal static bool ConditionIsGood(AssetSummaryDetail initialSection) => initialSection.ValuePerNumericAttribute["MINCOND"] >= 7;
        internal static bool ConditionIsGood(AssetDetail section) => section.ValuePerNumericAttribute["MINCOND"] >= 7;

        internal static bool ConditionIsFair(AssetSummaryDetail initialSection) => initialSection.ValuePerNumericAttribute["MINCOND"] < 7 && initialSection.ValuePerNumericAttribute["MINCOND"] >= 5;
        internal static bool ConditionIsFair(AssetDetail section) => section.ValuePerNumericAttribute["MINCOND"] < 7 && section.ValuePerNumericAttribute["MINCOND"] >= 5;

        internal static bool ConditionIsPoor(AssetSummaryDetail initialSection) => initialSection.ValuePerNumericAttribute["MINCOND"] < 5;
        internal static bool ConditionIsPoor(AssetDetail section) => section.ValuePerNumericAttribute["MINCOND"] < 5;

        internal static bool IsClosed(AssetSummaryDetail initialSection) => initialSection.ValuePerNumericAttribute["MINCOND"] < 3;
        internal static bool IsClosed(AssetDetail section) => section.ValuePerNumericAttribute["MINCOND"] < 3;

        internal static bool IsPosted(AssetSummaryDetail initialSection) => initialSection.ValuePerNumericAttribute["MINCOND"] >= 3 && initialSection.ValuePerNumericAttribute["SUP_SEEDED"] <= 4.1;
        internal static bool IsPosted(AssetDetail section) => section.ValuePerNumericAttribute["MINCOND"] >= 3 && section.ValuePerNumericAttribute["SUP_SEEDED"] <= 4.1;

        #endregion Private methods

        private static class CountAndAreaOfBridges
        {
            internal static int GetTotalInitialPoorCount(List<AssetSummaryDetail> initialAssetSummaries)
            {
                var count = 0;
                foreach (var initialSection in initialAssetSummaries)
                {
                    count += ConditionIsPoor(initialSection) ? 1 : 0;
                }
                return count;
            }

            internal static int GetTotalSectionalPoorCount(List<AssetDetail> sections)
            {
                var count = 0;
                foreach (var section in sections)
                {
                    count += ConditionIsPoor(section) ? 1 : 0;
                }
                return count;
            }

            internal static double GetTotalInitialPoorDeckArea(List<AssetSummaryDetail> initialAssetSummaries)
            {
                double sum = 0;
                foreach (var initialSection in initialAssetSummaries)
                {
                    var deckArea = ConditionIsPoor(initialSection) ? initialSection.ValuePerNumericAttribute["DECK_AREA"] : 0;
                    sum += deckArea;
                }

                return sum;
            }

            internal static double TotalPoorBridgesDeckArea(List<AssetDetail> sections, List<AssetSummaryDetail> initialAssetSummaries)
            {
                double sum = 0;
                foreach (var section in sections)
                {
                    var initialAssetSummary = initialAssetSummaries.FirstOrDefault(_ => _.AssetId == section.AssetId);
                    var deckArea = ConditionIsPoor(section) ? initialAssetSummary.ValuePerNumericAttribute["DECK_AREA"] : 0;
                    sum += deckArea;
                }

                return sum;
            }
        }
    }
}
