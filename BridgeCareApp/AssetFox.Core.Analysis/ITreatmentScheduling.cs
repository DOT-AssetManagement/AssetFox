namespace AssetFox.Core.Analysis;

internal interface ITreatmentScheduling
{
    int OffsetToFutureYear { get; }

    Treatment TreatmentToSchedule { get; }
}
