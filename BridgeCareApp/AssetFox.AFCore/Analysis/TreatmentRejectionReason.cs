namespace AssetFox.AFCore.Analysis
{
    public enum TreatmentRejectionReason
    {
        Undefined,
        WithinShadowForAnyTreatment,
        WithinShadowForSameTreatment,
        NotFeasible,
        Superseded,
    }
}
