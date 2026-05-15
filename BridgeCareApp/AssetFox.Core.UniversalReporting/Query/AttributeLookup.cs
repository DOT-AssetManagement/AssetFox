using AssetFox.Core.DTOs;

namespace AssetFox.Core.UniversalReporting.Query;

public sealed class AttributeLookup
{
    private readonly IReadOnlyDictionary<string, AttributeDTO> _byName;

    public AttributeLookup(IEnumerable<AttributeDTO> attributes)
    {
        _byName = attributes.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetByName(string name, out AttributeDTO? attribute) =>
        _byName.TryGetValue(name, out attribute);

    public IReadOnlyCollection<AttributeDTO> All => _byName.Values.ToList();
}
