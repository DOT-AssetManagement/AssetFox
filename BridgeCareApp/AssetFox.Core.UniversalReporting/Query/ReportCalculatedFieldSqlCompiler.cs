using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AssetFox.CalculateEvaluate;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Schema;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace AssetFox.Core.UniversalReporting.Query;

public enum ReportCalculatedFieldRowScope
{
    Summary,
    SummaryByMaintainableAsset,
    Yearly
}

public sealed class ReportCalculatedFieldProjectionPlan
{
    public string CteSql { get; init; } = "";
    public string ValuesCteName { get; init; } = "CalculatedFieldValues";
    public IReadOnlyList<ReportCalculatedFieldProjection> Fields { get; init; } = Array.Empty<ReportCalculatedFieldProjection>();
    public bool IsEmpty => Fields.Count == 0;

    public ReportCalculatedFieldProjection? FindById(string? fieldId) =>
        Fields.FirstOrDefault(field => string.Equals(field.FieldId, fieldId, StringComparison.OrdinalIgnoreCase));

    public ReportCalculatedFieldProjection? FindByColumnKey(string? columnKey) =>
        Fields.FirstOrDefault(field => string.Equals(field.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));

    public string Join(string parentAlias, string parentIdColumn = "Id", string planAlias = "cfv") =>
        IsEmpty ? "" : $"LEFT JOIN {ValuesCteName} {planAlias} ON {planAlias}.Id = {parentAlias}.{parentIdColumn}";

    public string SelectForCarry(string planAlias = "cfv") => string.Join(
        "",
        Fields.Select(field => $", {planAlias}.[{field.ColumnKey}] AS [{field.ColumnKey}]"));

    public string Carry(string parentAlias) => string.Join(
        "",
        Fields.Select(field => $", {parentAlias}.[{field.ColumnKey}]"));

    public string FinalSelect(string parentAlias) => string.Join(
        Environment.NewLine,
        Fields.Select(field => $"    , {parentAlias}.[{field.ColumnKey}] AS [{field.ColumnKey}]"));
}

public sealed record ReportCalculatedFieldProjection(
    string FieldId,
    string Name,
    string ResultType,
    string ColumnKey);

/// <summary>
/// Compiles report-scoped formulas and ordered rules to parameterized SQL. It intentionally
/// supports only the CalculateEvaluate expression subset that can be translated deterministically
/// to SQL Server. No user-authored SQL is ever included in a query.
/// </summary>
public sealed class ReportCalculatedFieldSqlCompiler
{
    private static readonly Regex BracketReferencePattern = new(@"\[([^\]]+)\]", RegexOptions.Compiled);
    private static readonly HashSet<string> NumberConstantNames = new(
        CalculateEvaluateCompiler.NumberConstantNames,
        StringComparer.OrdinalIgnoreCase);

    public ReportCalculatedFieldProjectionPlan BuildProjection(
        IEnumerable<ReportCalculatedField> catalog,
        IEnumerable<string> selectedFieldIds,
        IAttributeRepository attributeRepository,
        ReportCalculatedFieldRowScope rowScope,
        List<SqlParameter> parameters,
        string sqlNamespace = "cf")
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(selectedFieldIds);
        ArgumentNullException.ThrowIfNull(attributeRepository);
        ArgumentNullException.ThrowIfNull(parameters);

        var ids = selectedFieldIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ids.Count == 0)
        {
            return new ReportCalculatedFieldProjectionPlan();
        }

        var catalogById = catalog
            .Where(field => !string.IsNullOrWhiteSpace(field.Id))
            .ToDictionary(field => field.Id, StringComparer.OrdinalIgnoreCase);
        var fields = ids.Select(id => catalogById.TryGetValue(id, out var field)
                ? field
                : throw new InvalidOperationException($"Calculated field '{id}' was not found in this report."))
            .ToList();

        ValidateFieldIdentities(fields);

        var expressionReferences = new Dictionary<ReportExpressionDefinition, Dictionary<string, AttributeDTO>>();
        var conditionAttributes = new Dictionary<ReportRuleCondition, AttributeDTO>();
        var dependencies = new Dictionary<Guid, AttributeDTO>();
        foreach (var field in fields)
        {
            if (field.Mode == "formula" && field.Formula != null)
            {
                var expression = field.Formula;
                var references = ResolveExpressionReferences(expression, attributeRepository);
                expressionReferences[expression] = references;
                foreach (var attribute in references.Values)
                {
                    dependencies[attribute.Id] = attribute;
                }
            }

            if (field.Mode == "rules")
            {
                foreach (var condition in field.Rules.SelectMany(rule => rule.Conditions))
                {
                    var attribute = ResolveAttributeReference(condition.Attribute, attributeRepository);
                    conditionAttributes[condition] = attribute;
                    dependencies[attribute.Id] = attribute;
                }
            }
        }

        var dependencyList = dependencies.Values.OrderBy(attribute => attribute.Name).ToList();
        var aliases = dependencyList
            .Select((attribute, index) => (attribute.Id, Alias: $"CfAttr{index}"))
            .ToDictionary(item => item.Id, item => item.Alias);

        if (!Regex.IsMatch(sqlNamespace, "^[A-Za-z][A-Za-z0-9_]*$"))
        {
            throw new ArgumentException("Calculated-field SQL namespace is invalid.", nameof(sqlNamespace));
        }

        var attributeParameterNames = new List<string>();
        for (var index = 0; index < dependencyList.Count; index++)
        {
            var parameterName = $"@{sqlNamespace}_attr_{index}";
            attributeParameterNames.Add(parameterName);
            parameters.Add(new SqlParameter(parameterName, dependencyList[index].Id));
        }

        string ResolveValue(AttributeDTO attribute)
        {
            var alias = aliases[attribute.Id];
            return rowScope == ReportCalculatedFieldRowScope.Yearly
                ? $"COALESCE(cfd.[{alias}], cfs.[{alias}])"
                : $"cfa.[{alias}]";
        }

        var projections = new List<ReportCalculatedFieldProjection>();
        var projectionSql = new StringBuilder();
        var literalCounter = 0;
        foreach (var field in fields)
        {
            var columnKey = ColumnKey(field.Id);
            var sql = CompileField(
                field,
                expressionReferences,
                conditionAttributes,
                ResolveValue,
                parameters,
                ref literalCounter,
                sqlNamespace);
            projectionSql.AppendLine($"        {sql} AS [{columnKey}],");
            projections.Add(new ReportCalculatedFieldProjection(field.Id, field.Name, field.ResultType, columnKey));
        }

        var projectionBody = projectionSql.ToString().TrimEnd().TrimEnd(',');
        var ctePrefix = $"{sqlNamespace}CalculatedField";
        var cteSql = BuildCtes(
            rowScope,
            dependencyList,
            aliases,
            attributeParameterNames,
            projectionBody,
            ctePrefix);

        return new ReportCalculatedFieldProjectionPlan
        {
            CteSql = cteSql,
            ValuesCteName = $"{ctePrefix}Values",
            Fields = projections
        };
    }

    public void ValidateDefinitions(
        IEnumerable<ReportCalculatedField> fields,
        IAttributeRepository attributeRepository)
    {
        var list = fields?.ToList() ?? new List<ReportCalculatedField>();
        ValidateFieldIdentities(list);
        var parameters = new List<SqlParameter>();
        _ = BuildProjection(
            list,
            list.Select(field => field.Id),
            attributeRepository,
            ReportCalculatedFieldRowScope.Yearly,
            parameters);
    }

    public void ValidateSchemaReferences(ReportSchemaDefinition schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        var catalog = (schema.CalculatedFields ?? new List<ReportCalculatedField>())
            .ToDictionary(field => field.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var component in schema.AllComponents)
        {
            var config = JObject.FromObject(component.Config ?? new Dictionary<string, object>());
            foreach (var item in config.DescendantsAndSelf().OfType<JObject>())
            {
                var source = item["source"]?.ToString();
                var type = item["type"]?.ToString();
                if (!string.Equals(source, "calculatedField", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(type, "calculatedField", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fieldId = item["fieldId"]?.ToString() ?? item["field"]?.ToString();
                ValidateReference(component, fieldId, catalog);

                if (string.Equals(item["role"]?.ToString(), "measure", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(catalog[fieldId!].ResultType, "number", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Component '{component.Title}' uses text calculated field '{catalog[fieldId!].Name}' as a numeric measure.");
                }
            }

            foreach (var property in config.DescendantsAndSelf().OfType<JProperty>())
            {
                var value = property.Value.Type == JTokenType.String ? property.Value.ToString() : "";
                const string prefix = "calculatedField:";
                if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fieldId = value[prefix.Length..];
                ValidateReference(component, fieldId, catalog);
                if (property.Name.Contains("aggregate", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(catalog[fieldId].ResultType, "number", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Component '{component.Title}' uses text calculated field '{catalog[fieldId].Name}' as a numeric aggregate.");
                }
            }
        }
    }

    private static void ValidateReference(
        ReportComponent component,
        string? fieldId,
        IReadOnlyDictionary<string, ReportCalculatedField> catalog)
    {
        if (string.IsNullOrWhiteSpace(fieldId) || !catalog.ContainsKey(fieldId))
        {
            throw new InvalidOperationException(
                $"Component '{component.Title}' references calculated field '{fieldId}', which is not defined in this report.");
        }
    }

    public static string ColumnKey(string fieldId)
    {
        if (!Guid.TryParse(fieldId, out var id))
        {
            throw new InvalidOperationException("Every calculated field id must be a GUID.");
        }

        return $"cf_{id:N}";
    }

    private static void ValidateFieldIdentities(IReadOnlyList<ReportCalculatedField> fields)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            if (!Guid.TryParse(field.Id, out _))
            {
                throw new InvalidOperationException("Every calculated field id must be a GUID.");
            }

            if (!ids.Add(field.Id))
            {
                throw new InvalidOperationException($"Calculated field id '{field.Id}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(field.Name))
            {
                throw new InvalidOperationException("Every calculated field must have a name.");
            }

            if (!names.Add(field.Name.Trim()))
            {
                throw new InvalidOperationException($"Calculated field name '{field.Name}' is duplicated.");
            }

            if (field.ResultType is not ("text" or "number"))
            {
                throw new InvalidOperationException($"Calculated field '{field.Name}' has an unsupported result type.");
            }

            if (field.Mode is not ("formula" or "rules"))
            {
                throw new InvalidOperationException($"Calculated field '{field.Name}' has an unsupported mode.");
            }

            if (field.Mode == "formula" && field.ResultType != "number")
            {
                throw new InvalidOperationException($"Formula field '{field.Name}' must return a number.");
            }

            if (field.Mode == "formula" && string.IsNullOrWhiteSpace(field.Formula?.Expression))
            {
                throw new InvalidOperationException($"Formula field '{field.Name}' does not have a formula.");
            }

            if (field.Mode == "rules")
            {
                if (field.Rules == null || field.Rules.Count == 0)
                {
                    throw new InvalidOperationException($"Rule field '{field.Name}' must contain at least one rule.");
                }

                var ruleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var rule in field.Rules)
                {
                    if (!Guid.TryParse(rule.Id, out _))
                    {
                        throw new InvalidOperationException($"Every rule in calculated field '{field.Name}' must have a GUID id.");
                    }

                    if (!ruleIds.Add(rule.Id))
                    {
                        throw new InvalidOperationException(
                            $"Calculated field '{field.Name}' contains duplicate rule ids.");
                    }

                    if (rule.LogicalOperator is not ("all" or "any"))
                    {
                        throw new InvalidOperationException(
                            $"Calculated field '{field.Name}' contains a rule with an unsupported condition match mode.");
                    }

                    if (rule.Conditions == null || rule.Conditions.Count == 0)
                    {
                        throw new InvalidOperationException($"Calculated field '{field.Name}' contains a rule without conditions.");
                    }

                    var conditionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var condition in rule.Conditions)
                    {
                        if (!Guid.TryParse(condition.Id, out _) || !conditionIds.Add(condition.Id))
                        {
                            throw new InvalidOperationException(
                                $"Every condition in calculated field '{field.Name}' must have a unique GUID id within its rule.");
                        }

                        if (condition.Attribute == null
                            || (string.IsNullOrWhiteSpace(condition.Attribute.AttributeId)
                                && string.IsNullOrWhiteSpace(condition.Attribute.Name)))
                        {
                            throw new InvalidOperationException(
                                $"Calculated field '{field.Name}' contains a condition without an attribute.");
                        }

                        if (string.IsNullOrWhiteSpace(condition.Operator))
                        {
                            throw new InvalidOperationException(
                                $"Calculated field '{field.Name}' contains a condition without an operator.");
                        }

                        if (string.IsNullOrWhiteSpace(condition.Value))
                        {
                            throw new InvalidOperationException(
                                $"Calculated field '{field.Name}' contains a condition without a comparison value.");
                        }
                    }
                }
            }
        }
    }

    private static AttributeDTO ResolveAttributeReference(
        ReportAttributeReference reference,
        IAttributeRepository attributeRepository)
    {
        AttributeDTO? attribute = null;
        if (Guid.TryParse(reference.AttributeId, out var attributeId))
        {
            attribute = attributeRepository.GetSingleById(attributeId);
        }

        attribute ??= attributeRepository.GetSingleByName(reference.Name);
        if (attribute == null)
        {
            throw new InvalidOperationException($"Attribute '{reference.Name}' used by a calculated field was not found.");
        }

        return attribute;
    }

    private static Dictionary<string, AttributeDTO> ResolveExpressionReferences(
        ReportExpressionDefinition definition,
        IAttributeRepository attributeRepository)
    {
        var result = new Dictionary<string, AttributeDTO>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in definition.AttributeReferences ?? new List<ReportAttributeReference>())
        {
            var attribute = ResolveAttributeReference(reference, attributeRepository);

            var token = string.IsNullOrWhiteSpace(reference.Token) ? reference.Name : reference.Token;
            result[token] = attribute;
        }

        foreach (Match match in BracketReferencePattern.Matches(definition.Expression ?? ""))
        {
            var token = match.Groups[1].Value;
            if (result.ContainsKey(token))
            {
                continue;
            }

            var attribute = attributeRepository.GetSingleByName(token);
            if (attribute == null)
            {
                throw new CalculateEvaluateCompilationException($"Unknown reference \"{token}\".");
            }

            result[token] = attribute;
        }

        return result;
    }

    private static string CompileField(
        ReportCalculatedField field,
        IReadOnlyDictionary<ReportExpressionDefinition, Dictionary<string, AttributeDTO>> references,
        IReadOnlyDictionary<ReportRuleCondition, AttributeDTO> conditionAttributes,
        Func<AttributeDTO, string> valueResolver,
        List<SqlParameter> parameters,
        ref int literalCounter,
        string sqlNamespace)
    {
        if (field.Mode == "formula")
        {
            var expression = field.Formula!;
            return CompileExpression(
                expression.Expression,
                references[expression],
                valueResolver,
                requireBoolean: false,
                parameters,
                ref literalCounter,
                sqlNamespace);
        }

        var clauses = new List<string>();
        var fieldDependencies = new Dictionary<Guid, AttributeDTO>();
        foreach (var rule in field.Rules)
        {
            var conditionClauses = new List<string>();
            foreach (var condition in rule.Conditions)
            {
                var attribute = conditionAttributes[condition];
                fieldDependencies[attribute.Id] = attribute;
                conditionClauses.Add(CompileCondition(
                    condition,
                    attribute,
                    valueResolver,
                    parameters,
                    ref literalCounter,
                    sqlNamespace));
            }

            var join = rule.LogicalOperator == "any" ? " OR " : " AND ";
            var criteriaSql = $"({string.Join(join, conditionClauses)})";
            var resultSql = AddResultParameter(field, rule.Result, parameters, ref literalCounter, sqlNamespace);
            clauses.Add($"WHEN {criteriaSql} THEN {resultSql}");
        }

        var defaultSql = AddResultParameter(field, field.DefaultResult, parameters, ref literalCounter, sqlNamespace);
        if (field.MissingResult != null && fieldDependencies.Count > 0)
        {
            var missingSql = AddResultParameter(field, field.MissingResult, parameters, ref literalCounter, sqlNamespace);
            var missingCondition = string.Join(
                " OR ",
                fieldDependencies.Values.Select(attribute => $"{valueResolver(attribute)} IS NULL"));
            clauses.Insert(0, $"WHEN ({missingCondition}) THEN {missingSql}");
        }

        return $"CASE {string.Join(" ", clauses)} ELSE {defaultSql} END";
    }

    private static string CompileCondition(
        ReportRuleCondition condition,
        AttributeDTO attribute,
        Func<AttributeDTO, string> valueResolver,
        List<SqlParameter> parameters,
        ref int literalCounter,
        string sqlNamespace)
    {
        var valueSql = valueResolver(attribute);
        var parameterName = $"@{sqlNamespace}_condition_{literalCounter++}";
        var isNumeric = string.Equals(attribute.Type, "NUMBER", StringComparison.OrdinalIgnoreCase);

        if (isNumeric)
        {
            if (!double.TryParse(condition.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                || !double.IsFinite(number))
            {
                throw new InvalidOperationException(
                    $"Comparison value '{condition.Value}' for attribute '{attribute.Name}' is not a valid number.");
            }

            var sqlOperator = condition.Operator switch
            {
                "equals" => "=",
                "notEquals" => "<>",
                "lessThan" => "<",
                "lessThanOrEqual" => "<=",
                "greaterThan" => ">",
                "greaterThanOrEqual" => ">=",
                _ => throw new InvalidOperationException(
                    $"Operator '{condition.Operator}' is not supported for numeric attribute '{attribute.Name}'.")
            };
            parameters.Add(new SqlParameter(parameterName, number));
            return $"({valueSql} {sqlOperator} {parameterName})";
        }

        parameters.Add(new SqlParameter(parameterName, condition.Value!));
        var trimmedValue = $"LTRIM(RTRIM({valueSql}))";
        var trimmedParameter = $"LTRIM(RTRIM({parameterName}))";
        return condition.Operator switch
        {
            "equals" => $"({trimmedValue} = {trimmedParameter})",
            "notEquals" => $"({trimmedValue} <> {trimmedParameter})",
            "contains" => $"(CHARINDEX({parameterName}, {valueSql}) > 0)",
            "notContains" => $"(CHARINDEX({parameterName}, {valueSql}) = 0)",
            "startsWith" => $"(LEFT({valueSql}, LEN({parameterName})) = {parameterName})",
            "endsWith" => $"(RIGHT({valueSql}, LEN({parameterName})) = {parameterName})",
            _ => throw new InvalidOperationException(
                $"Operator '{condition.Operator}' is not supported for text attribute '{attribute.Name}'.")
        };
    }

    private static string AddResultParameter(
        ReportCalculatedField field,
        ReportCalculationResult? result,
        List<SqlParameter> parameters,
        ref int literalCounter,
        string sqlNamespace)
    {
        if (result?.Value == null)
        {
            return field.ResultType == "number"
                ? "CAST(NULL AS FLOAT)"
                : "CAST(NULL AS NVARCHAR(4000))";
        }

        var parameterName = $"@{sqlNamespace}_result_{literalCounter++}";
        if (field.ResultType == "number")
        {
            if (!double.TryParse(result.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue)
                || !double.IsFinite(numericValue))
            {
                throw new InvalidOperationException(
                    $"Calculated field '{field.Name}' result '{result.Value}' is not a valid number.");
            }

            parameters.Add(new SqlParameter(parameterName, numericValue));
        }
        else
        {
            parameters.Add(new SqlParameter(parameterName, result.Value));
        }

        return parameterName;
    }

    private static string CompileExpression(
        string expression,
        IReadOnlyDictionary<string, AttributeDTO> references,
        Func<AttributeDTO, string> valueResolver,
        bool requireBoolean,
        List<SqlParameter> parameters,
        ref int literalCounter,
        string sqlNamespace)
    {
        var tree = Parse(expression);
        var visitor = new SqlVisitor(references, valueResolver, parameters, sqlNamespace, literalCounter);
        var result = visitor.Visit(tree);
        literalCounter = visitor.LiteralCounter;
        var expected = requireBoolean ? ExpressionType.Boolean : ExpressionType.Number;
        if (result.Type != expected)
        {
            throw new CalculateEvaluateCompilationException(
                requireBoolean
                    ? "Rule criteria must be a boolean expression."
                    : "Formula must be a numeric expression.");
        }

        return result.Sql;
    }

    private static IParseTree Parse(string expression)
    {
        var input = new AntlrInputStream(expression ?? "");
        var lexer = new CalculateEvaluateLexer(input);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener<int>.Instance);
        var tokens = new CommonTokenStream(lexer);
        var parser = new CalculateEvaluateParser(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingErrorListener<IToken>.Instance);
        parser.ErrorHandler = new BailErrorStrategy();

        try
        {
            return parser.root();
        }
        catch (ParseCanceledException exception)
        {
            throw new CalculateEvaluateParsingException("Failed to parse calculated field expression.", exception);
        }
    }

    private static string BuildCtes(
        ReportCalculatedFieldRowScope scope,
        IReadOnlyList<AttributeDTO> dependencies,
        IReadOnlyDictionary<Guid, string> aliases,
        IReadOnlyList<string> attributeParameterNames,
        string projectionBody,
        string ctePrefix)
    {
        var attributeParams = attributeParameterNames;
        var attributesFilter = attributeParams.Count == 0
            ? ""
            : $"\n      AND v.AttributeId IN ({string.Join(", ", attributeParams)})";
        var summaryAttributesFilter = attributeParams.Count == 0
            ? ""
            : $"\n      AND sv.AttributeId IN ({string.Join(", ", attributeParams)})";

        string PivotBody(string tableAlias) => string.Join(
            ",\n",
            dependencies.Select((attribute, parameterIndex) =>
            {
                var alias = aliases[attribute.Id];
                var valueColumn = attribute.Type == "NUMBER" ? "NumericValue" : "TextValue";
                return $"        MAX(CASE WHEN {tableAlias}.AttributeId = {attributeParameterNames[parameterIndex]} THEN {tableAlias}.{valueColumn} END) AS [{alias}]";
            }));

        if (scope is ReportCalculatedFieldRowScope.Summary or ReportCalculatedFieldRowScope.SummaryByMaintainableAsset)
        {
            var byMaintainableAsset = scope == ReportCalculatedFieldRowScope.SummaryByMaintainableAsset;
            var rowIdExpression = byMaintainableAsset ? "asd.MaintainableAssetId" : "asd.Id";
            var valueIdExpression = byMaintainableAsset ? "asd.MaintainableAssetId" : "v.AssetSummaryDetailId";
            var valueJoin = byMaintainableAsset
                ? @"
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = v.RunId AND asd.Id = v.AssetSummaryDetailId"
                : "";
            var attributeCte = dependencies.Count == 0
                ? ""
                : $@",
{ctePrefix}Attrs AS (
    SELECT {valueIdExpression} AS Id,
{PivotBody("v")}
    FROM dbo.AssetSummaryDetailValueIntId v
    {valueJoin}
    WHERE v.RunId = @RunId{attributesFilter}
    GROUP BY {valueIdExpression}
)";
            var attributeJoin = dependencies.Count == 0
                ? ""
                : $"\n    LEFT JOIN {ctePrefix}Attrs cfa ON cfa.Id = cfr.Id";
            return $@"{ctePrefix}Rows AS (
    SELECT DISTINCT {rowIdExpression} AS Id
    FROM dbo.AssetSummaryDetail asd
    WHERE asd.RunId = @RunId
){attributeCte},
{ctePrefix}Values AS (
    SELECT cfr.Id,
{projectionBody}
    FROM {ctePrefix}Rows cfr{attributeJoin}
)";
        }

        var detailCte = dependencies.Count == 0
            ? ""
            : $@",
{ctePrefix}DetailAttrs AS (
    SELECT v.AssetDetailId AS Id,
{PivotBody("v")}
    FROM dbo.AssetDetailValueIntId v
    WHERE v.RunId = @RunId{attributesFilter}
    GROUP BY v.AssetDetailId
),
{ctePrefix}SummaryAttrs AS (
    SELECT asd.MaintainableAssetId,
{PivotBody("sv")}
    FROM dbo.AssetSummaryDetailValueIntId sv
    INNER JOIN dbo.AssetSummaryDetail asd
        ON asd.RunId = sv.RunId AND asd.Id = sv.AssetSummaryDetailId
    WHERE sv.RunId = @RunId{summaryAttributesFilter}
    GROUP BY asd.MaintainableAssetId
)";
        var detailJoins = dependencies.Count == 0
            ? ""
            : $@"
    LEFT JOIN {ctePrefix}DetailAttrs cfd ON cfd.Id = cfr.Id
    LEFT JOIN {ctePrefix}SummaryAttrs cfs ON cfs.MaintainableAssetId = cfr.MaintainableAssetId";
        return $@"{ctePrefix}Rows AS (
    SELECT ad.Id, ad.MaintainableAssetId
    FROM dbo.AssetDetail ad
    WHERE ad.RunId = @RunId
){detailCte},
{ctePrefix}Values AS (
    SELECT cfr.Id,
{projectionBody}
    FROM {ctePrefix}Rows cfr{detailJoins}
)";
    }

    private enum ExpressionType
    {
        Number,
        Text,
        Boolean,
        Literal
    }

    private readonly record struct SqlExpression(string Sql, ExpressionType Type, string? Literal = null);

    private sealed class SqlVisitor : CalculateEvaluateParserBaseVisitor<SqlExpression>
    {
        private readonly IReadOnlyDictionary<string, AttributeDTO> _references;
        private readonly Func<AttributeDTO, string> _valueResolver;
        private readonly List<SqlParameter> _parameters;
        private readonly string _sqlNamespace;
        public int LiteralCounter { get; private set; }

        public SqlVisitor(
            IReadOnlyDictionary<string, AttributeDTO> references,
            Func<AttributeDTO, string> valueResolver,
            List<SqlParameter> parameters,
            string sqlNamespace,
            int literalCounter)
        {
            _references = references;
            _valueResolver = valueResolver;
            _parameters = parameters;
            _sqlNamespace = sqlNamespace;
            LiteralCounter = literalCounter;
        }

        public override SqlExpression VisitCalculationRoot(CalculateEvaluateParser.CalculationRootContext context) =>
            Visit(context.calculation());

        public override SqlExpression VisitEvaluationRoot(CalculateEvaluateParser.EvaluationRootContext context) =>
            Visit(context.evaluation());

        public override SqlExpression VisitAdditionOrSubtraction(CalculateEvaluateParser.AdditionOrSubtractionContext context)
        {
            var operation = context.operation.Type == CalculateEvaluateLexer.PLUS ? "+" : "-";
            return Arithmetic(Visit(context.left), Visit(context.right), operation);
        }

        public override SqlExpression VisitMultiplicationOrDivision(CalculateEvaluateParser.MultiplicationOrDivisionContext context)
        {
            var left = RequireNumber(Visit(context.left));
            var right = RequireNumber(Visit(context.right));
            return context.operation.Type == CalculateEvaluateLexer.TIMES
                ? new SqlExpression($"({left.Sql} * {right.Sql})", ExpressionType.Number)
                : new SqlExpression($"({left.Sql} / NULLIF({right.Sql}, 0))", ExpressionType.Number);
        }

        public override SqlExpression VisitNegation(CalculateEvaluateParser.NegationContext context)
        {
            var operand = RequireNumber(Visit(context.calculation()));
            return new SqlExpression($"(-{operand.Sql})", ExpressionType.Number);
        }

        public override SqlExpression VisitCalculationGrouping(CalculateEvaluateParser.CalculationGroupingContext context) =>
            Parenthesize(Visit(context.calculation()));

        public override SqlExpression VisitEvaluationGrouping(CalculateEvaluateParser.EvaluationGroupingContext context) =>
            Parenthesize(Visit(context.evaluation()));

        public override SqlExpression VisitNumberLiteral(CalculateEvaluateParser.NumberLiteralContext context)
        {
            var text = context.NUMBER().GetText();
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                || !double.IsFinite(number))
            {
                throw new CalculateEvaluateCompilationException($"Failed to parse literal \"{text}\".");
            }

            return new SqlExpression(number.ToString("R", CultureInfo.InvariantCulture), ExpressionType.Number);
        }

        public override SqlExpression VisitNumberReference(CalculateEvaluateParser.NumberReferenceContext context)
        {
            var name = context.IDENTIFIER().GetText();
            if (NumberConstantNames.Contains(name))
            {
                var field = typeof(Math).GetField(
                    name,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.IgnoreCase);
                return new SqlExpression(
                    Convert.ToDouble(field!.GetValue(null), CultureInfo.InvariantCulture)
                        .ToString("R", CultureInfo.InvariantCulture),
                    ExpressionType.Number);
            }

            return Reference(name, requireNumber: true);
        }

        public override SqlExpression VisitNumberParameterReference(CalculateEvaluateParser.NumberParameterReferenceContext context) =>
            Reference(context.IDENTIFIER().GetText(), requireNumber: true);

        public override SqlExpression VisitParameterReference(CalculateEvaluateParser.ParameterReferenceContext context)
        {
            var nested = context.parameterReference();
            return nested != null
                ? VisitParameterReference(nested)
                : Reference(context.IDENTIFIER().GetText(), requireNumber: false);
        }

        public override SqlExpression VisitLiteral(CalculateEvaluateParser.LiteralContext context) =>
            new("", ExpressionType.Literal, context.content?.Text ?? "");

        public override SqlExpression VisitInvocation(CalculateEvaluateParser.InvocationContext context)
        {
            var name = context.IDENTIFIER().GetText();
            var args = context.arguments().calculation().Select(argument => RequireNumber(Visit(argument))).ToList();
            var sqlName = name.ToLowerInvariant() switch
            {
                "abs" => "ABS",
                "ceiling" => "CEILING",
                "floor" => "FLOOR",
                "sqrt" => "SQRT",
                "pow" => "POWER",
                "round" => "ROUND",
                "truncate" => "ROUND",
                _ => throw new CalculateEvaluateCompilationException($"Function \"{name}\" is not supported in report formulas.")
            };

            if (name.Equals("truncate", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Count != 1)
                {
                    throw new CalculateEvaluateCompilationException("Truncate requires one argument.");
                }

                return new SqlExpression($"ROUND({args[0].Sql}, 0, 1)", ExpressionType.Number);
            }

            var expectedCounts = name.ToLowerInvariant() switch
            {
                "pow" => new[] { 2 },
                "round" => new[] { 1, 2 },
                _ => new[] { 1 }
            };
            if (!expectedCounts.Contains(args.Count))
            {
                throw new CalculateEvaluateCompilationException($"Function \"{name}\" has an invalid number of arguments.");
            }

            if (name.Equals("round", StringComparison.OrdinalIgnoreCase) && args.Count == 1)
            {
                args.Add(new SqlExpression("0", ExpressionType.Number));
            }

            return new SqlExpression($"{sqlName}({string.Join(", ", args.Select(argument => argument.Sql))})", ExpressionType.Number);
        }

        public override SqlExpression VisitEqual(CalculateEvaluateParser.EqualContext context) =>
            Compare(context.left, context.right, "=", allowText: true);

        public override SqlExpression VisitNotEqual(CalculateEvaluateParser.NotEqualContext context) =>
            Compare(context.left, context.right, "<>", allowText: true);

        public override SqlExpression VisitLessThan(CalculateEvaluateParser.LessThanContext context) =>
            Compare(context.left, context.right, "<", allowText: false);

        public override SqlExpression VisitLessThanOrEqual(CalculateEvaluateParser.LessThanOrEqualContext context) =>
            Compare(context.left, context.right, "<=", allowText: false);

        public override SqlExpression VisitGreaterThan(CalculateEvaluateParser.GreaterThanContext context) =>
            Compare(context.left, context.right, ">", allowText: false);

        public override SqlExpression VisitGreaterThanOrEqual(CalculateEvaluateParser.GreaterThanOrEqualContext context) =>
            Compare(context.left, context.right, ">=", allowText: false);

        public override SqlExpression VisitLogicalConjunction(CalculateEvaluateParser.LogicalConjunctionContext context) =>
            Logical(Visit(context.left), Visit(context.right), "AND");

        public override SqlExpression VisitLogicalDisjunction(CalculateEvaluateParser.LogicalDisjunctionContext context) =>
            Logical(Visit(context.left), Visit(context.right), "OR");

        private SqlExpression Reference(string token, bool requireNumber)
        {
            if (!_references.TryGetValue(token, out var attribute))
            {
                throw new CalculateEvaluateCompilationException($"Unknown reference \"{token}\".");
            }

            var isNumber = attribute.Type == "NUMBER";
            if (requireNumber && !isNumber)
            {
                throw new CalculateEvaluateCompilationException($"Parameter \"{token}\" is not a number.");
            }

            return new SqlExpression(
                _valueResolver(attribute),
                isNumber ? ExpressionType.Number : ExpressionType.Text);
        }

        private SqlExpression Arithmetic(SqlExpression left, SqlExpression right, string operation)
        {
            left = RequireNumber(left);
            right = RequireNumber(right);
            return new SqlExpression($"({left.Sql} {operation} {right.Sql})", ExpressionType.Number);
        }

        private SqlExpression Compare(
            CalculateEvaluateParser.ComparisonOperandContext leftContext,
            CalculateEvaluateParser.ComparisonOperandContext rightContext,
            string operation,
            bool allowText)
        {
            var left = Visit(leftContext);
            var right = Visit(rightContext);
            if (left.Type == ExpressionType.Literal && right.Type == ExpressionType.Literal)
            {
                throw new CalculateEvaluateCompilationException("Comparison operands are both literals.");
            }

            if (left.Type == ExpressionType.Literal)
            {
                left = ConvertLiteral(left, right.Type);
            }
            else if (right.Type == ExpressionType.Literal)
            {
                right = ConvertLiteral(right, left.Type);
            }

            if (left.Type != right.Type)
            {
                throw new CalculateEvaluateCompilationException("Comparison operands are not the same type.");
            }

            if (left.Type == ExpressionType.Text)
            {
                if (!allowText)
                {
                    throw new CalculateEvaluateCompilationException("Ordering comparison is not supported for text operands.");
                }

                return new SqlExpression(
                    $"(LTRIM(RTRIM({left.Sql})) {operation} LTRIM(RTRIM({right.Sql})))",
                    ExpressionType.Boolean);
            }

            return new SqlExpression($"({left.Sql} {operation} {right.Sql})", ExpressionType.Boolean);
        }

        private SqlExpression ConvertLiteral(SqlExpression literal, ExpressionType target)
        {
            if (target == ExpressionType.Number)
            {
                if (!double.TryParse(literal.Literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                    || !double.IsFinite(number))
                {
                    throw new CalculateEvaluateCompilationException($"Failed to parse literal \"{literal.Literal}\".");
                }

                return new SqlExpression(number.ToString("R", CultureInfo.InvariantCulture), ExpressionType.Number);
            }

            var parameterName = $"@{_sqlNamespace}_literal_{LiteralCounter++}";
            _parameters.Add(new SqlParameter(parameterName, literal.Literal ?? ""));
            return new SqlExpression(parameterName, ExpressionType.Text);
        }

        private static SqlExpression Logical(SqlExpression left, SqlExpression right, string operation)
        {
            if (left.Type != ExpressionType.Boolean || right.Type != ExpressionType.Boolean)
            {
                throw new CalculateEvaluateCompilationException($"Logical {operation} requires boolean operands.");
            }

            return new SqlExpression($"({left.Sql} {operation} {right.Sql})", ExpressionType.Boolean);
        }

        private SqlExpression RequireNumber(SqlExpression expression)
        {
            if (expression.Type == ExpressionType.Literal)
            {
                return ConvertLiteral(expression, ExpressionType.Number);
            }

            if (expression.Type != ExpressionType.Number)
            {
                throw new CalculateEvaluateCompilationException("Arithmetic operands must be numeric.");
            }

            return expression;
        }

        private static SqlExpression Parenthesize(SqlExpression expression) =>
            new($"({expression.Sql})", expression.Type, expression.Literal);
    }

    private sealed class ThrowingErrorListener<TSymbol> : IAntlrErrorListener<TSymbol>
    {
        public static readonly ThrowingErrorListener<TSymbol> Instance = new();

        public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            TSymbol offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e) =>
            throw new CalculateEvaluateParsingException(
                $"Syntax error at line {line}, column {charPositionInLine}: {msg}",
                e);
    }
}
