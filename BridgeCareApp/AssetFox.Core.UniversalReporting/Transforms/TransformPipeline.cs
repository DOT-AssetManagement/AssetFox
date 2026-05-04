using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Transforms;

public sealed class TransformPipeline
{
    private readonly IReadOnlyList<IRowTransform> _transforms;

    public TransformPipeline(IEnumerable<IRowTransform> transforms)
    {
        _transforms = transforms.ToList();
    }

    public bool RequiresMaterialization(ReportComponent component) =>
        _transforms.Any(transform => transform.AppliesTo(component));

    public (IReadOnlyList<ViewerColumn> Columns, IReadOnlyList<List<object>> Rows) Apply(
        ReportComponent component,
        IReadOnlyList<ViewerColumn> columns,
        IReadOnlyList<List<object>> rows)
    {
        var currentColumns = columns;
        var currentRows = rows;

        foreach (var transform in _transforms.Where(transform => transform.AppliesTo(component)))
        {
            (currentColumns, currentRows) = transform.Apply(component, currentColumns, currentRows);
        }

        return (currentColumns, currentRows);
    }
}
