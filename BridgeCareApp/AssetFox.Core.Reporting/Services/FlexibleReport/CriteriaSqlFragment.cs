using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public class CriteriaSqlFragment
    {
        public string WhereClause { get; set; }
        public List<SqlParameter> Parameters { get; set; } = new();
        public bool IsEmpty => string.IsNullOrWhiteSpace(WhereClause);
    }
}
