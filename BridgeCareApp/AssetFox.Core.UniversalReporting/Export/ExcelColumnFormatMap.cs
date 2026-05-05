namespace AssetFox.Core.UniversalReporting.Export;

public static class ExcelColumnFormatMap
{
    /// <summary>
    /// Maps universal report column formats to Excel number formats. Returning null leaves
    /// the column with the workbook's default "General" formatting.
    /// </summary>
    public static string? Resolve(string? format) =>
        format?.ToLowerInvariant() switch
        {
            "currency" => "$#,##0.00",
            "number" => "#,##0.##",
            "percent" => "0.0\"%\"",
            _ => null
        };
}
