namespace AssetFox.Core.UniversalReporting.Components;

public class ComponentDataSourceRegistry
{
    private readonly IReadOnlyDictionary<string, IComponentDataSource> _dataSources;

    public ComponentDataSourceRegistry(IEnumerable<IComponentDataSource> dataSources)
    {
        _dataSources = dataSources.ToDictionary(
            dataSource => dataSource.ComponentType,
            StringComparer.OrdinalIgnoreCase);
    }

    public IComponentDataSource For(string? componentType)
    {
        if (string.IsNullOrWhiteSpace(componentType))
        {
            throw new InvalidOperationException("Report component type is required.");
        }

        if (_dataSources.TryGetValue(componentType, out var dataSource))
        {
            return dataSource;
        }

        throw new NotSupportedException($"Report component type '{componentType}' is not supported.");
    }

    public bool TryGet(string? componentType, out IComponentDataSource? dataSource)
    {
        dataSource = null;
        return !string.IsNullOrWhiteSpace(componentType)
            && _dataSources.TryGetValue(componentType, out dataSource);
    }
}
