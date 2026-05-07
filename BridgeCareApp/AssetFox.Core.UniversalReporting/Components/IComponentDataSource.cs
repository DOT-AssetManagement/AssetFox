using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;

namespace AssetFox.Core.UniversalReporting.Components;

public interface IComponentDataSource
{
    string ComponentType { get; }

    ComponentQuery Build(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes);
}
