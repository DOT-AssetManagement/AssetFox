namespace AssetFox.AFCore.Analysis
{
    public enum ReasonAgainstCashFlow
    {
        Undefined,
        NotNeeded,
        ApplicableDistributionRuleIsForOnlyOneYear,
        LastYearOfCashFlowIsOutsideOfAnalysisPeriod,
        FutureEventScheduleIsBlocked,
        FutureFundingIsNotAvailable,
        None,
    }
}
