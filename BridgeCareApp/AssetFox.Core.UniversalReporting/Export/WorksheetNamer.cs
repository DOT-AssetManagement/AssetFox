namespace AssetFox.Core.UniversalReporting.Export;

public sealed class WorksheetNamer
{
    private readonly HashSet<string> _usedNames = new(StringComparer.OrdinalIgnoreCase);

    public string Name(string? preferredName, int fallbackIndex)
    {
        var baseName = Sanitize(preferredName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = $"Section {fallbackIndex + 1}";
        }

        baseName = Truncate(baseName, 31);
        var candidate = baseName;
        var duplicateIndex = 1;

        while (!_usedNames.Add(candidate))
        {
            var suffix = $" ({duplicateIndex++})";
            var prefixLength = Math.Max(1, 31 - suffix.Length);
            candidate = $"{Truncate(baseName, prefixLength)}{suffix}";
        }

        return candidate;
    }

    private static string Sanitize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var invalidChars = new HashSet<char>(new[] { ':', '\\', '/', '?', '*', '[', ']' });
        return new string(name.Trim()
                .Select(c => invalidChars.Contains(c) ? '-' : c)
                .ToArray())
            .Trim('\'')
            .Trim();
    }

    private static string Truncate(string name, int maxLength) =>
        string.IsNullOrEmpty(name) || name.Length <= maxLength
            ? name
            : name[..maxLength];
}
