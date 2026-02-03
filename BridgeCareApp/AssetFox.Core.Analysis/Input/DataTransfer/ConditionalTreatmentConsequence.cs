namespace AssetFox.Core.Analysis.Input.DataTransfer;

public sealed class ConditionalTreatmentConsequence : TreatmentConsequence
{
    public string CriterionExpression { get; set; }

    public string EquationExpression { get; set; }
}
