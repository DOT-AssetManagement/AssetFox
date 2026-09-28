using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetFox.Core.UniversalReporting.Charts;

/// <summary>
/// Applies a chart's value styling (colors and order for the legend field's values).
/// Keep in sync with report-chart-styling.ts, which applies the same rules in the viewer.
/// </summary>
public static class ChartValueStyler
{
    public const int MaxStyledValues = 30;
    public const string NoneValue = "(None)";

    public static readonly IReadOnlySet<string> SupportedOrders =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "listed", "alphabetical", "alphabeticalDesc", "valueDesc" };

    private static readonly Regex HexColor = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);
    private static readonly Regex NaturalParts = new("[0-9]+|[^0-9]+", RegexOptions.Compiled);

    public static string BindingKey(ChartFieldBindingDefinition? binding) =>
        binding == null || string.IsNullOrWhiteSpace(binding.Source) || string.IsNullOrWhiteSpace(binding.Field)
            ? ""
            : $"{binding.Source}:{binding.Field}";

    /// <summary>The field whose values are styled: the series when one is bound, otherwise the category.</summary>
    public static string? StyledRole(ChartConfigDefinition config)
    {
        if (!string.IsNullOrWhiteSpace(config.Bindings.Series?.Field)) return "series";
        if (!string.IsNullOrWhiteSpace(config.Bindings.Category?.Field)) return "category";
        return null;
    }

    /// <summary>The chart's styling, or null when there is none or it was set up for a different field.</summary>
    public static ChartValueStylingDefinition? ActiveStyling(ChartConfigDefinition config)
    {
        var styling = config.ValueStyling;
        var role = StyledRole(config);
        if (styling == null || role == null)
        {
            return null;
        }

        var binding = role == "series" ? config.Bindings.Series : config.Bindings.Category;
        var key = BindingKey(binding);
        return key.Length > 0 && string.Equals(styling.BindingKey, key, StringComparison.Ordinal) ? styling : null;
    }

    public static bool IsValidColor(string? color) => color != null && HexColor.IsMatch(color);

    /// <summary>Orders the values present in the chart. Listed values come first (in list order) for "listed".</summary>
    public static List<string> Order(
        IReadOnlyList<string> values,
        ChartValueStylingDefinition? styling,
        Func<string, double>? total = null)
    {
        if (styling == null)
        {
            return values.ToList();
        }

        var byName = values.OrderBy(value => value, NaturalComparer.Instance).ToList();
        switch (styling.Order?.ToLowerInvariant())
        {
            case "alphabetical":
                return byName;
            case "alphabeticaldesc":
                byName.Reverse();
                return byName.OrderBy(IsNone).ToList();
            case "valuedesc":
                return byName.OrderByDescending(value => total?.Invoke(value) ?? 0).ToList();
            default:
                var rank = new Dictionary<string, int>(StringComparer.Ordinal);
                for (var i = 0; i < styling.Values.Count; i++)
                {
                    rank.TryAdd(Normalize(styling.Values[i].Value), i);
                }

                var listed = values
                    .Where(value => rank.ContainsKey(Normalize(value)))
                    .OrderBy(value => rank[Normalize(value)]);
                return listed.Concat(byName.Where(value => !rank.ContainsKey(Normalize(value)))).ToList();
        }
    }

    /// <summary>
    /// Colors for the values present in the chart: listed values use their color, others take automatic colors from
    /// the palette, skipping colors already assigned so they don't clash.
    /// </summary>
    public static List<Color> ResolveColors(
        IReadOnlyList<string> values,
        ChartValueStylingDefinition styling,
        IReadOnlyList<Color> palette)
    {
        var explicitColors = new Dictionary<string, Color>(StringComparer.Ordinal);
        foreach (var style in styling.Values.Where(style => IsValidColor(style.Color)))
        {
            explicitColors.TryAdd(Normalize(style.Value), ParseColor(style.Color));
        }

        var used = explicitColors.Values.Select(color => color.ToArgb()).ToHashSet();
        var unused = palette.Where(color => !used.Contains(color.ToArgb())).ToList();
        var automatic = unused.Count > 0 ? unused : palette.ToList();
        var next = 0;
        return values
            .Select(value => explicitColors.TryGetValue(Normalize(value), out var color)
                ? color
                : automatic[next++ % automatic.Count])
            .ToList();
    }

    public static string Normalize(string? value) => (value ?? "").Trim().ToLowerInvariant();

    private static bool IsNone(string value) => Normalize(value) == Normalize(NoneValue);

    private static Color ParseColor(string hex) => Color.FromArgb(
        int.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber),
        int.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber),
        int.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber));

    /// <summary>Natural, case-insensitive order ("2" before "10"), with "(None)" after everything else.</summary>
    public sealed class NaturalComparer : IComparer<string>
    {
        public static readonly NaturalComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            var xNone = IsNone(x ?? "");
            var yNone = IsNone(y ?? "");
            if (xNone != yNone) return xNone ? 1 : -1;

            var xParts = NaturalParts.Matches(Normalize(x)).Select(match => match.Value).ToList();
            var yParts = NaturalParts.Matches(Normalize(y)).Select(match => match.Value).ToList();
            for (var i = 0; i < Math.Min(xParts.Count, yParts.Count); i++)
            {
                var result = ComparePart(xParts[i], yParts[i]);
                if (result != 0) return result;
            }

            return xParts.Count - yParts.Count;
        }

        private static int ComparePart(string x, string y)
        {
            if (char.IsAsciiDigit(x[0]) && char.IsAsciiDigit(y[0]))
            {
                var xTrimmed = TrimLeadingZeros(x);
                var yTrimmed = TrimLeadingZeros(y);
                if (xTrimmed.Length != yTrimmed.Length) return xTrimmed.Length - yTrimmed.Length;
            }

            return Math.Sign(string.CompareOrdinal(x, y));
        }

        private static string TrimLeadingZeros(string digits)
        {
            var trimmed = digits.TrimStart('0');
            return trimmed.Length == 0 ? "0" : trimmed;
        }
    }
}
