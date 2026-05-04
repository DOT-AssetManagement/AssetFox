using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Transforms;

public interface IRowTransform
{
    bool AppliesTo(ReportComponent component);

    (IReadOnlyList<ViewerColumn> Columns, IReadOnlyList<List<object>> Rows) Apply(
        ReportComponent component,
        IReadOnlyList<ViewerColumn> columns,
        IReadOnlyList<List<object>> rows);
}
