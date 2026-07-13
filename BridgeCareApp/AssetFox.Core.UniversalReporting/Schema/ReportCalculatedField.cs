namespace AssetFox.Core.UniversalReporting.Schema;

public sealed class ReportCalculatedField
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string ResultType { get; set; } = "text";
    public string Mode { get; set; } = "rules";
    public ReportExpressionDefinition? Formula { get; set; }
    public List<ReportCalculationRule> Rules { get; set; } = new();
    public ReportCalculationResult? DefaultResult { get; set; }
    public ReportCalculationResult? MissingResult { get; set; }
}

public sealed class ReportExpressionDefinition
{
    public string Expression { get; set; } = "";
    public List<ReportAttributeReference> AttributeReferences { get; set; } = new();
}

public sealed class ReportAttributeReference
{
    public string AttributeId { get; set; } = "";
    public string Token { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class ReportCalculationRule
{
    public string Id { get; set; } = "";
    public string LogicalOperator { get; set; } = "all";
    public List<ReportRuleCondition> Conditions { get; set; } = new();
    public ReportCalculationResult Result { get; set; } = new();
}

public sealed class ReportRuleCondition
{
    public string Id { get; set; } = "";
    public ReportAttributeReference Attribute { get; set; } = new();
    public string Operator { get; set; } = "";
    public string? Value { get; set; }
}

public sealed class ReportCalculationResult
{
    public string? Value { get; set; }
}
