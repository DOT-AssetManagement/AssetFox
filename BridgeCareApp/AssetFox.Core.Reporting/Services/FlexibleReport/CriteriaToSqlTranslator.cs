using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    /// <summary>
    /// Translates bracket-based criteria expressions (e.g., [DISTRICT] = '5' AND [DECK_SEEDED] &lt; 4)
    /// into parameterized SQL WHERE clause fragments with EAV EXISTS subqueries.
    /// </summary>
    public class CriteriaToSqlTranslator
    {
        private static readonly Regex AttributePattern = new(@"\[([^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex TokenPattern = new(
            @"\[([^\]]+)\]" +                           // [ATTR_NAME]
            @"|(\bAND\b)" +                              // AND
            @"|(\bOR\b)" +                               // OR
            @"|(<>|<=|>=|<|>|=)" +                       // comparison operators
            @"|(\()" +                                    // open paren
            @"|(\))" +                                    // close paren
            @"|\|([^|]*)\|" +                            // |pipe-delimited string|
            @"|'([^']*)'" +                              // 'single-quoted string'
            @"|(-?\d+\.?\d*)" +                          // numeric literal
            @"|(\S+)",                                    // catch-all token
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Translates a criteria expression into a SQL WHERE clause fragment that uses
        /// EXISTS subqueries against the specified EAV value table.
        /// </summary>
        /// <param name="criteriaExpression">The bracket-based criteria expression.</param>
        /// <param name="attributeRepo">Repository for looking up attribute metadata.</param>
        /// <param name="valueTableName">The EAV value table name (e.g., "AssetDetailValueIntId" or "AssetSummaryDetailValueIntId").</param>
        /// <param name="parentIdColumn">The parent FK column in the value table (e.g., "AssetDetailId" or "AssetSummaryDetailId").</param>
        /// <param name="parentAlias">The alias used for the parent table in the outer query (e.g., "ad").</param>
        /// <param name="paramPrefix">Prefix for parameter names to avoid collisions (e.g., "sc" for schema criteria, "vf" for viewer filter).</param>
        /// <returns>A CriteriaSqlFragment with the WHERE clause and parameters, or an empty fragment if the expression is blank.</returns>
        public CriteriaSqlFragment Translate(
            string criteriaExpression,
            IAttributeRepository attributeRepo,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            string paramPrefix = "cr")
        {
            var fragment = new CriteriaSqlFragment();

            if (string.IsNullOrWhiteSpace(criteriaExpression))
                return fragment;

            // Step 1: Extract all unique attribute names from the expression
            var attributeNames = AttributePattern.Matches(criteriaExpression)
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!attributeNames.Any())
                return fragment;

            // Step 2: Look up attribute metadata (Id, Type)
            var attributes = attributeRepo.GetAttributesWithNames(attributeNames);
            var attrLookup = new Dictionary<string, AttributeDTO>(StringComparer.OrdinalIgnoreCase);
            foreach (var attr in attributes)
            {
                attrLookup[attr.Name] = attr;
            }

            // Register attribute ID parameters
            foreach (var kvp in attrLookup)
            {
                var paramName = $"@{paramPrefix}_attr_{SanitizeParamName(kvp.Key)}";
                fragment.Parameters.Add(new SqlParameter(paramName, kvp.Value.Id));
            }

            // Step 3: Token-walk the expression and build SQL
            var sql = new StringBuilder();
            var matches = TokenPattern.Matches(criteriaExpression);
            int paramCounter = 0;
            string pendingAttribute = null;
            string pendingOperator = null;

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) // [ATTR_NAME]
                {
                    pendingAttribute = match.Groups[1].Value;
                }
                else if (match.Groups[2].Success) // AND
                {
                    sql.Append(" AND ");
                }
                else if (match.Groups[3].Success) // OR
                {
                    sql.Append(" OR ");
                }
                else if (match.Groups[4].Success) // comparison operator
                {
                    pendingOperator = match.Groups[4].Value;
                }
                else if (match.Groups[5].Success) // open paren
                {
                    sql.Append("(");
                }
                else if (match.Groups[6].Success) // close paren
                {
                    sql.Append(")");
                }
                else if (match.Groups[7].Success || match.Groups[8].Success) // string literal (pipe or single-quoted)
                {
                    string value = match.Groups[7].Success ? match.Groups[7].Value : match.Groups[8].Value;

                    if (pendingAttribute != null && pendingOperator != null)
                    {
                        var existsClause = BuildExistsClause(
                            pendingAttribute, pendingOperator, value, true,
                            attrLookup, valueTableName, parentIdColumn, parentAlias,
                            paramPrefix, ref paramCounter, fragment.Parameters);
                        sql.Append(existsClause);
                        pendingAttribute = null;
                        pendingOperator = null;
                    }
                }
                else if (match.Groups[9].Success) // numeric literal
                {
                    string value = match.Groups[9].Value;

                    if (pendingAttribute != null && pendingOperator != null)
                    {
                        var existsClause = BuildExistsClause(
                            pendingAttribute, pendingOperator, value, false,
                            attrLookup, valueTableName, parentIdColumn, parentAlias,
                            paramPrefix, ref paramCounter, fragment.Parameters);
                        sql.Append(existsClause);
                        pendingAttribute = null;
                        pendingOperator = null;
                    }
                }
                // Group 10 is catch-all, skip unknown tokens
            }

            fragment.WhereClause = sql.ToString().Trim();
            return fragment;
        }

        /// <summary>
        /// Builds a simple column filter SQL fragment for viewer-level interactive filters.
        /// Supports filtering on both simulation fields (direct columns) and EAV attributes.
        /// </summary>
        public CriteriaSqlFragment TranslateColumnFilters(
            Dictionary<string, string> columnFilters,
            IAttributeRepository attributeRepo,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            List<string> simulationFieldColumns)
        {
            var fragment = new CriteriaSqlFragment();

            if (columnFilters == null || !columnFilters.Any())
                return fragment;

            var clauses = new List<string>();
            int paramCounter = 0;

            foreach (var filter in columnFilters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                if (simulationFieldColumns.Contains(filter.Key, StringComparer.OrdinalIgnoreCase))
                {
                    // Direct column filter (simulation fields like AppliedTreatment, Year, etc.)
                    var paramName = $"@vf_col_{paramCounter}";
                    var filterValue = $"%{filter.Value}%";
                    fragment.Parameters.Add(new SqlParameter(paramName, filterValue));

                    var columnName = MapSimulationFieldToColumn(filter.Key);
                    clauses.Add($"CAST({parentAlias}.{columnName} AS NVARCHAR(200)) LIKE {paramName}");
                    paramCounter++;
                }
                else
                {
                    // EAV attribute filter
                    var attr = attributeRepo.GetSingleByName(filter.Key);
                    if (attr == null) continue;

                    var attrParamName = $"@vf_attr_{paramCounter}";
                    fragment.Parameters.Add(new SqlParameter(attrParamName, attr.Id));

                    if (attr.Type == "NUMBER")
                    {
                        // Try parse as number for exact/range comparison, fall back to LIKE
                        if (double.TryParse(filter.Value, out var numVal))
                        {
                            var valParamName = $"@vf_val_{paramCounter}";
                            fragment.Parameters.Add(new SqlParameter(valParamName, numVal));
                            clauses.Add($"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                                $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                                $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 'n' " +
                                $"AND v.NumericValue = {valParamName})");
                        }
                        else
                        {
                            var valParamName = $"@vf_val_{paramCounter}";
                            fragment.Parameters.Add(new SqlParameter(valParamName, $"%{filter.Value}%"));
                            clauses.Add($"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                                $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                                $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 'n' " +
                                $"AND CAST(v.NumericValue AS NVARCHAR(100)) LIKE {valParamName})");
                        }
                    }
                    else
                    {
                        var valParamName = $"@vf_val_{paramCounter}";
                        fragment.Parameters.Add(new SqlParameter(valParamName, $"%{filter.Value}%"));
                        clauses.Add($"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                            $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                            $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 't' " +
                            $"AND v.TextValue LIKE {valParamName})");
                    }

                    paramCounter++;
                }
            }

            if (clauses.Any())
                fragment.WhereClause = string.Join(" AND ", clauses);

            return fragment;
        }

        private string BuildExistsClause(
            string attributeName,
            string op,
            string value,
            bool isStringLiteral,
            Dictionary<string, AttributeDTO> attrLookup,
            string valueTableName,
            string parentIdColumn,
            string parentAlias,
            string paramPrefix,
            ref int paramCounter,
            List<SqlParameter> parameters)
        {
            if (!attrLookup.TryGetValue(attributeName, out var attr))
                return "1=0"; // Unknown attribute — match nothing

            var attrParamName = $"@{paramPrefix}_attr_{SanitizeParamName(attributeName)}";
            var valParamName = $"@{paramPrefix}_val_{paramCounter}";
            paramCounter++;

            bool isNumericAttr = attr.Type == "NUMBER";

            if (isNumericAttr)
            {
                double numValue = 0;
                if (isStringLiteral)
                    double.TryParse(value, out numValue);
                else
                    double.TryParse(value, out numValue);

                parameters.Add(new SqlParameter(valParamName, numValue));

                return $"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                       $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                       $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 'n' " +
                       $"AND v.NumericValue {op} {valParamName})";
            }
            else
            {
                parameters.Add(new SqlParameter(valParamName, value));

                string comparison;
                if (op == "=" || op == "<>")
                {
                    comparison = $"LTRIM(RTRIM(v.TextValue)) {op} LTRIM(RTRIM({valParamName}))";
                }
                else
                {
                    comparison = $"v.TextValue {op} {valParamName}";
                }

                return $"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                       $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                       $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 't' " +
                       $"AND {comparison})";
            }
        }

        /// <summary>
        /// Builds a set-based sort join for an EAV attribute column.
        /// Returns the sort column reference (wk._SortVal) and a LEFT JOIN subquery to inject
        /// into the WithKey CTE that pre-computes the sort value once over all FilteredAssets rows.
        /// This replaces the previous correlated scalar subquery inside ROW_NUMBER() OVER (ORDER BY ...)
        /// which executed once per row in WithKey (potentially hundreds of thousands of rows).
        /// </summary>
        public (string SortColumnRef, string WithKeyJoin) BuildAttributeSortExpression(
            string attributeName,
            IAttributeRepository attributeRepo,
            string valueTableName,
            string parentIdColumn,
            List<SqlParameter> parameters)
        {
            var attr = attributeRepo.GetSingleByName(attributeName);
            if (attr == null)
                return ("fa.Id", "");

            var paramName = "@sort_attr";
            var existing = parameters.FirstOrDefault(p => p.ParameterName == paramName);
            if (existing != null) parameters.Remove(existing);
            parameters.Add(new SqlParameter(paramName, attr.Id));

            string valueColumn = attr.Type == "NUMBER" ? "NumericValue" : "TextValue";

            // One set-based scan of the EAV table for all FilteredAssets IDs
            // Result column _SortVal is carried through WithKey → SortedAssets → ORDER BY
            string withKeyJoin = $@"LEFT JOIN (
        SELECT {parentIdColumn} AS _SortId,
            MAX(CASE WHEN AttributeId = {paramName} THEN {valueColumn} END) AS _SortVal
        FROM dbo.{valueTableName}
        WHERE RunId = @RunId
          AND {parentIdColumn} IN (SELECT Id FROM FilteredAssets)
        GROUP BY {parentIdColumn}
    ) _sv ON _sv._SortId = fa.Id";

            return ("wk._SortVal", withKeyJoin);
        }

        private static string MapSimulationFieldToColumn(string fieldKey)
        {
            return fieldKey.ToLowerInvariant() switch
            {
                "year" => "Year",
                "appliedtreatment" => "AppliedTreatment",
                "treatmentcause" => "TreatmentCause",
                "treatmentstatus" => "TreatmentStatus",
                "budgetname" => "BudgetName",
                "goalname" => "GoalName",
                _ => fieldKey
            };
        }

        private static string SanitizeParamName(string name)
        {
            return Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }
    }
}
