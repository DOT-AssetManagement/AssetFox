using System;

namespace AssetFox.Core.Analysis.Engine;

internal sealed class TreatmentProgress
{
    public TreatmentProgress(Treatment treatment)
    {
        Treatment = treatment ?? throw new ArgumentNullException(nameof(treatment));
    }

    public bool IsComplete { get; set; }

    public Treatment Treatment { get; }
}
