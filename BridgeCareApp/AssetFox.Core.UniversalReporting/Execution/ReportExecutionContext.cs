using AssetFox.Core.UniversalReporting.Schema;

namespace AssetFox.Core.UniversalReporting.Execution;

public sealed record ReportExecutionContext(
    ReportComponent Component,
    IReadOnlyList<ReportCalculatedField> CalculatedFields)
{
    public static ReportExecutionContext Create(
        ReportComponent component,
        IEnumerable<ReportCalculatedField>? calculatedFields = null) =>
        new(component, calculatedFields?.ToList() ?? new List<ReportCalculatedField>());
}
