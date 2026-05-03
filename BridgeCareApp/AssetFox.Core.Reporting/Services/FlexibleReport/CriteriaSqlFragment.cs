using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public class CriteriaSqlFragment
    {
        public string WhereClause { get; set; }
        public List<SqlParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Pivot CTE definition that materializes the criteria-referenced attributes once
        /// per asset. Empty when the expression references no attributes. Format is
        /// "CriteriaAttrs AS (...)" — the caller prepends to its WITH chain.
        /// </summary>
        public string CriteriaCte { get; set; }

        /// <summary>
        /// JOIN clause that splices CriteriaAttrs into the FilteredAssets FROM clause.
        /// Empty when no CTE is needed.
        /// </summary>
        public string CriteriaJoin { get; set; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(WhereClause);
    }
}
