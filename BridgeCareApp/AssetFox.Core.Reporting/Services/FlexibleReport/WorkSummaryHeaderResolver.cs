namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    // Maps (aggregateField, aggregateFunction) to a human-readable column title
    // and cell format. Invalid combinations fall back to the only sensible
    // aggregation for that field, so the rendered header always matches what
    // the SQL/aggregation actually computes. Used by both the live viewer
    // (ViewerDataService) and the Excel export path (DataExtractor) so the
    // two stay in sync.
    public static class WorkSummaryHeaderResolver
    {
        public static (string Header, string Format) Resolve(string aggregateField, string aggregateFunction)
        {
            var field = (aggregateField ?? "").ToLowerInvariant();
            var func = (aggregateFunction ?? "").ToLowerInvariant();

            if (field == "cost")
            {
                return func switch
                {
                    "sum" => ("Total Cost", "currency"),
                    "avg" => ("Average Cost", "currency"),
                    "min" => ("Minimum Cost", "currency"),
                    "max" => ("Maximum Cost", "currency"),
                    _ => ("Total Cost", "currency"),
                };
            }

            return ("Asset Count", "number");
        }
    }
}
