using System;

namespace AppliedResearchAssociates.iAM.DataUnitTests
{
    public class TestAttributeIds
    {
        public const string AgeIdString = "d27f24d1-7f8a-4778-a2b2-e61911a58897";
        public static Guid AgeId = Guid.Parse(AgeIdString);

        public const string ConditionIndexIdString = "82D39C6C-229C-468A-BB3C-AC2B95145FFB";
        public static Guid ConditionIndexId => Guid.Parse(ConditionIndexIdString);


        public const string CulvDurationNIdString  = "efca598b-9fca-4e3c-ac48-0d95a9eaa867";
        public static Guid CulvDurationNId = Guid.Parse(CulvDurationNIdString);

        public const string BrKeyIdString = "547CE06B-08E4-4D5D-9215-D5A22EDD5DC7";
        public static Guid BrKeyId = Guid.Parse(BrKeyIdString);

        public const string BmsIdString = "24C42A9F-3E80-4D3A-9E95-CB60063F3CAA";
        public static Guid BmsidId = Guid.Parse(BmsIdString);

        public const string DistrictIdString = "B279B58E-89CC-4662-8874-C87F04ADB60E";
        public static Guid DistrictId = Guid.Parse(DistrictIdString);

        public const string RiskScoreIdString = "F6A260B0-B94D-4737-971B-4DE25EF36DF2";
        public static Guid RiskScoreId => Guid.Parse(RiskScoreIdString);
    }
}
