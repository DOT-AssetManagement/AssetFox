using System.Collections.Generic;

namespace AssetFox.Core.Analysis.Input.DataTransfer;

public sealed class CalculatedField : Attribute
{
    public bool IsDecreasingWithDeterioration { get; set; }

    public CalculatedFieldTiming Timing { get; set; }

    public List<CriterionEquationPair> ValueDefinitions { get; init; } = new();
}
