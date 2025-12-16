using System.Diagnostics.Eventing.Reader;
using AppliedResearchAssociates.iAM.Analysis.Engine;

namespace AppliedResearchAssociates.iAM.Reporting.Services.BAMSSummaryReport.DistrictCountyTotals
{
    public static class DistrictTotalsSectionDetailPredicates
    {
        public static bool IsTurnpike(AssetDetail section, AssetSummaryDetail assetSummary)
        {
            var ownerCode = assetSummary.ValuePerTextAttribute["OWNER_CODE"];
            var returnValue = ownerCode.Trim() == "31";
            return returnValue;
        }

        public static bool IsCommittedProject(AssetDetail section)
        {
            var returnValue = section.TreatmentCause == TreatmentCause.CommittedProject;
            return returnValue;
        }

        public static bool IsDistrictNotTurnpike(AssetDetail section, AssetSummaryDetail assetSummary, int districtNumber)
        {
            var actualDistrict = assetSummary.ValuePerTextAttribute["DISTRICT"];
            var isTurnpike = IsTurnpike(section, assetSummary);
            var returnValue = !isTurnpike && int.TryParse(actualDistrict, out var sectionDistrict) && sectionDistrict == districtNumber;
            return returnValue;
        }

        public static bool IsNumberedDistrictMpmsTable(AssetDetail section, AssetSummaryDetail assetSummary, int districtNumber)
        {
            var committed = IsCommittedProject(section);
            var district = IsDistrictNotTurnpike(section, assetSummary, districtNumber);
            return district && committed;
        }

        public static bool IsCounty(AssetDetail section, AssetSummaryDetail assetSummary, string county)
        {
            var actualCounty = assetSummary.ValuePerTextAttribute["COUNTY"];
            var returnValue = actualCounty.ToUpper() == county.ToUpper();
            return returnValue;
        }

        public static bool IsNumberedDistrictBamsTable(AssetDetail section, AssetSummaryDetail assetSummary, int districtNumber)
        {
            var ownerCode = assetSummary.ValuePerTextAttribute["OWNER_CODE"];
            var committed = IsCommittedProject(section);
            var district = IsDistrictNotTurnpike(section, assetSummary, districtNumber);
            return district && !committed;
        }

        public static bool IsCommittedTurnpike(AssetDetail section, AssetSummaryDetail assetSummary)
        {
            bool committed = IsCommittedProject(section);
            bool turnpike = IsTurnpike(section, assetSummary);
            return committed && turnpike;
        }


        public static bool IsTurnpikeButNotCommitted(AssetDetail section, AssetSummaryDetail assetSummary)
        {
            bool committed = IsCommittedProject(section);
            bool turnpike = IsTurnpike(section, assetSummary);
            return turnpike && !committed;
        }
    }
}
