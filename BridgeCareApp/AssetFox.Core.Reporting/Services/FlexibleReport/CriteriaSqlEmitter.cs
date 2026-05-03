using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AssetFox.CalculateEvaluate;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using Microsoft.Data.SqlClient;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    /// <summary>
    /// Translates bracket-based criteria expressions into SQL by walking the same ANTLR parse
    /// tree the simulation engine uses (<see cref="CalculateEvaluateCompiler"/>). Referenced
    /// attributes are pivoted once into a CriteriaAttrs CTE so arithmetic across attributes
    /// (e.g. [A] + [B] &lt; 10) composes naturally; the WHERE clause then references CTE columns
    /// rather than re-scanning the EAV per comparison. This is the only place criteria are
    /// translated for the report viewer; if the engine accepts an expression, this emitter
    /// must accept it (or throw a precise error). Silent token-dropping is forbidden.
    /// </summary>
    public class CriteriaSqlEmitter
    {
        private static readonly Regex AttributePattern = new(@"\[([^\]]+)\]", RegexOptions.Compiled);
        private static readonly HashSet<string> NumberConstantNames = new(
            CalculateEvaluateCompiler.NumberConstantNames, StringComparer.OrdinalIgnoreCase);

        public CriteriaSqlFragment EmitWhere(
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

            var bracketNames = AttributePattern.Matches(criteriaExpression)
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var resolved = new Dictionary<string, AttributeDTO>(StringComparer.OrdinalIgnoreCase);
            if (bracketNames.Count > 0)
            {
                foreach (var attr in attributeRepo.GetAttributesWithNames(bracketNames))
                    resolved[attr.Name] = attr;

                foreach (var name in bracketNames)
                {
                    if (!resolved.ContainsKey(name))
                        throw new CalculateEvaluateCompilationException($"Unknown reference \"{name}\".");
                }
            }

            var tree = ParseEvaluation(criteriaExpression);

            var visitor = new EmittingVisitor(resolved, paramPrefix, fragment.Parameters);
            var rootResult = visitor.Visit(tree);
            if (rootResult.Type != ExprType.Boolean)
                throw new CalculateEvaluateCompilationException("Criteria expression must be a boolean expression.");

            fragment.WhereClause = rootResult.Sql;

            if (visitor.ReferencedAttributes.Count > 0)
            {
                AddAttributeIdParameters(fragment.Parameters, visitor.ReferencedAttributes, paramPrefix);
                fragment.CriteriaCte = BuildCriteriaAttrsCte(
                    visitor.ReferencedAttributes, valueTableName, parentIdColumn, paramPrefix);
                fragment.CriteriaJoin = $"INNER JOIN CriteriaAttrs ca ON ca.Id = {parentAlias}.Id";
            }

            return fragment;
        }

        public CriteriaSqlFragment EmitColumnFilterWhere(
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
                    var paramName = $"@vf_col_{paramCounter}";
                    fragment.Parameters.Add(new SqlParameter(paramName, $"%{filter.Value}%"));
                    var columnName = MapSimulationFieldToColumn(filter.Key);
                    clauses.Add($"CAST({parentAlias}.{columnName} AS NVARCHAR(200)) LIKE {paramName}");
                    paramCounter++;
                }
                else
                {
                    var attr = attributeRepo.GetSingleByName(filter.Key);
                    if (attr == null) continue;

                    var attrParamName = $"@vf_attr_{paramCounter}";
                    fragment.Parameters.Add(new SqlParameter(attrParamName, attr.Id));

                    if (attr.Type == "NUMBER")
                    {
                        var valParamName = $"@vf_val_{paramCounter}";
                        if (double.TryParse(filter.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var numVal))
                        {
                            fragment.Parameters.Add(new SqlParameter(valParamName, numVal));
                            clauses.Add($"EXISTS (SELECT 1 FROM dbo.{valueTableName} v " +
                                $"WHERE v.RunId = @RunId AND v.{parentIdColumn} = {parentAlias}.Id " +
                                $"AND v.AttributeId = {attrParamName} AND v.Discriminator = 'n' " +
                                $"AND v.NumericValue = {valParamName})");
                        }
                        else
                        {
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

        public (string SortColumnRef, string WithKeyJoin) EmitAttributeSortJoin(
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

        private static IParseTree ParseEvaluation(string expression)
        {
            var input = new AntlrInputStream(expression);
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
            catch (ParseCanceledException parseCanceled)
            {
                string message = "Failed to parse criteria expression.";
                if (parseCanceled.InnerException is RecognitionException recognition && recognition.OffendingToken != null)
                {
                    var t = recognition.OffendingToken;
                    message = $"Offending token \"{t.Text}\" at line {t.Line}, column {t.Column}.";
                }
                throw new CalculateEvaluateParsingException(message, parseCanceled);
            }
        }

        private static void AddAttributeIdParameters(
            List<SqlParameter> parameters,
            IReadOnlyDictionary<string, AttributeDTO> referenced,
            string paramPrefix)
        {
            foreach (var kvp in referenced)
            {
                var paramName = $"@{paramPrefix}_attr_{SanitizeParamName(kvp.Key)}";
                if (!parameters.Any(p => p.ParameterName == paramName))
                    parameters.Add(new SqlParameter(paramName, kvp.Value.Id));
            }
        }

        private static string BuildCriteriaAttrsCte(
            IReadOnlyDictionary<string, AttributeDTO> referenced,
            string valueTableName,
            string parentIdColumn,
            string paramPrefix)
        {
            var pivotLines = new StringBuilder();
            var attrIdParams = new List<string>();
            foreach (var (name, attr) in referenced)
            {
                var attrParam = $"@{paramPrefix}_attr_{SanitizeParamName(name)}";
                attrIdParams.Add(attrParam);
                if (attr.Type == "NUMBER")
                {
                    pivotLines.AppendLine(
                        $"        MAX(CASE WHEN v.AttributeId = {attrParam} THEN v.NumericValue END) AS [{name}_n],");
                }
                else
                {
                    pivotLines.AppendLine(
                        $"        MAX(CASE WHEN v.AttributeId = {attrParam} THEN v.TextValue END) AS [{name}_t],");
                }
            }
            string pivotBody = pivotLines.ToString().TrimEnd().TrimEnd(',');

            return
$@"CriteriaAttrs AS (
    SELECT v.{parentIdColumn} AS Id,
{pivotBody}
    FROM dbo.{valueTableName} v
    WHERE v.RunId = @RunId
      AND v.AttributeId IN ({string.Join(", ", attrIdParams)})
    GROUP BY v.{parentIdColumn}
)";
        }

        private static string MapSimulationFieldToColumn(string fieldKey) => fieldKey.ToLowerInvariant() switch
        {
            "year" => "Year",
            "appliedtreatment" => "AppliedTreatment",
            "treatmentcause" => "TreatmentCause",
            "treatmentstatus" => "TreatmentStatus",
            "budgetname" => "BudgetName",
            "goalname" => "GoalName",
            _ => fieldKey
        };

        private static string SanitizeParamName(string name) =>
            Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");

        private enum ExprType { Number, Text, Boolean }

        private readonly struct EmittedExpr
        {
            public string Sql { get; }
            public ExprType Type { get; }
            public bool IsLiteral { get; }
            public string LiteralText { get; }

            public EmittedExpr(string sql, ExprType type, bool isLiteral = false, string literalText = null)
            {
                Sql = sql;
                Type = type;
                IsLiteral = isLiteral;
                LiteralText = literalText;
            }
        }

        private sealed class EmittingVisitor : CalculateEvaluateParserBaseVisitor<EmittedExpr>
        {
            private readonly IReadOnlyDictionary<string, AttributeDTO> _attributes;
            private readonly string _paramPrefix;
            private readonly List<SqlParameter> _parameters;
            private int _literalCounter;

            public Dictionary<string, AttributeDTO> ReferencedAttributes { get; } =
                new(StringComparer.OrdinalIgnoreCase);

            public EmittingVisitor(
                IReadOnlyDictionary<string, AttributeDTO> attributes,
                string paramPrefix,
                List<SqlParameter> parameters)
            {
                _attributes = attributes;
                _paramPrefix = paramPrefix;
                _parameters = parameters;
            }

            public override EmittedExpr VisitEvaluationRoot(CalculateEvaluateParser.EvaluationRootContext context) =>
                Visit(context.evaluation());

            public override EmittedExpr VisitCalculationRoot(CalculateEvaluateParser.CalculationRootContext context) =>
                throw new CalculateEvaluateCompilationException("Criteria expression must be a boolean expression, not a calculation.");

            public override EmittedExpr VisitEvaluationGrouping(CalculateEvaluateParser.EvaluationGroupingContext context)
            {
                var inner = Visit(context.evaluation());
                return new EmittedExpr($"({inner.Sql})", inner.Type);
            }

            public override EmittedExpr VisitLogicalConjunction(CalculateEvaluateParser.LogicalConjunctionContext context) =>
                EmitLogical(context.left, context.right, "AND");

            public override EmittedExpr VisitLogicalDisjunction(CalculateEvaluateParser.LogicalDisjunctionContext context) =>
                EmitLogical(context.left, context.right, "OR");

            private EmittedExpr EmitLogical(
                CalculateEvaluateParser.EvaluationContext left,
                CalculateEvaluateParser.EvaluationContext right,
                string op)
            {
                var leftExpr = Visit(left);
                var rightExpr = Visit(right);
                if (leftExpr.Type != ExprType.Boolean || rightExpr.Type != ExprType.Boolean)
                    throw new CalculateEvaluateCompilationException($"Logical {op} requires boolean operands.");
                return new EmittedExpr($"({leftExpr.Sql} {op} {rightExpr.Sql})", ExprType.Boolean);
            }

            public override EmittedExpr VisitEqual(CalculateEvaluateParser.EqualContext context) =>
                EmitComparison(context.left, context.right, "=", allowText: true);

            public override EmittedExpr VisitNotEqual(CalculateEvaluateParser.NotEqualContext context) =>
                EmitComparison(context.left, context.right, "<>", allowText: true);

            public override EmittedExpr VisitLessThan(CalculateEvaluateParser.LessThanContext context) =>
                EmitComparison(context.left, context.right, "<", allowText: false);

            public override EmittedExpr VisitLessThanOrEqual(CalculateEvaluateParser.LessThanOrEqualContext context) =>
                EmitComparison(context.left, context.right, "<=", allowText: false);

            public override EmittedExpr VisitGreaterThan(CalculateEvaluateParser.GreaterThanContext context) =>
                EmitComparison(context.left, context.right, ">", allowText: false);

            public override EmittedExpr VisitGreaterThanOrEqual(CalculateEvaluateParser.GreaterThanOrEqualContext context) =>
                EmitComparison(context.left, context.right, ">=", allowText: false);

            private EmittedExpr EmitComparison(
                CalculateEvaluateParser.ComparisonOperandContext leftCtx,
                CalculateEvaluateParser.ComparisonOperandContext rightCtx,
                string op,
                bool allowText)
            {
                var leftExpr = Visit(leftCtx);
                var rightExpr = Visit(rightCtx);

                if (leftExpr.IsLiteral && rightExpr.IsLiteral)
                    throw new CalculateEvaluateCompilationException("Comparison operands are both literals.");

                ExprType operandType;
                if (leftExpr.IsLiteral)
                {
                    operandType = rightExpr.Type;
                    leftExpr = MaterializeLiteral(leftExpr, operandType);
                }
                else if (rightExpr.IsLiteral)
                {
                    operandType = leftExpr.Type;
                    rightExpr = MaterializeLiteral(rightExpr, operandType);
                }
                else if (leftExpr.Type != rightExpr.Type)
                {
                    throw new CalculateEvaluateCompilationException("Comparison operands are not the same type.");
                }
                else
                {
                    operandType = leftExpr.Type;
                }

                if (operandType == ExprType.Text)
                {
                    if (!allowText)
                        throw new CalculateEvaluateCompilationException("Ordering comparison is not supported for text operands.");

                    return new EmittedExpr(
                        $"(LTRIM(RTRIM({leftExpr.Sql})) {op} LTRIM(RTRIM({rightExpr.Sql})))",
                        ExprType.Boolean);
                }

                return new EmittedExpr($"({leftExpr.Sql} {op} {rightExpr.Sql})", ExprType.Boolean);
            }

            private EmittedExpr MaterializeLiteral(EmittedExpr literal, ExprType targetType)
            {
                var paramName = $"@{_paramPrefix}_lit_{_literalCounter++}";
                if (targetType == ExprType.Number)
                {
                    if (!double.TryParse(literal.LiteralText, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                        throw new CalculateEvaluateCompilationException($"Failed to parse literal \"{literal.LiteralText}\".");
                    _parameters.Add(new SqlParameter(paramName, num));
                    return new EmittedExpr(paramName, ExprType.Number);
                }
                _parameters.Add(new SqlParameter(paramName, literal.LiteralText ?? ""));
                return new EmittedExpr(paramName, ExprType.Text);
            }

            public override EmittedExpr VisitParameterReference(CalculateEvaluateParser.ParameterReferenceContext context)
            {
                var inner = context.parameterReference();
                if (inner != null) return VisitParameterReference(inner);

                var name = context.IDENTIFIER().GetText();
                return ReferenceAttribute(name, requireNumber: false);
            }

            public override EmittedExpr VisitLiteral(CalculateEvaluateParser.LiteralContext context)
            {
                var text = context.content?.Text ?? "";
                return new EmittedExpr(sql: null, type: ExprType.Text, isLiteral: true, literalText: text);
            }

            public override EmittedExpr VisitNumberLiteral(CalculateEvaluateParser.NumberLiteralContext context)
            {
                var text = context.NUMBER().GetText();
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                    throw new CalculateEvaluateCompilationException($"Failed to parse literal \"{text}\".");
                return new EmittedExpr(num.ToString("R", CultureInfo.InvariantCulture), ExprType.Number);
            }

            public override EmittedExpr VisitNumberReference(CalculateEvaluateParser.NumberReferenceContext context)
            {
                var name = context.IDENTIFIER().GetText();
                if (NumberConstantNames.Contains(name))
                {
                    var value = (double)typeof(Math).GetField(name,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.IgnoreCase).GetValue(null);
                    return new EmittedExpr(value.ToString("R", CultureInfo.InvariantCulture), ExprType.Number);
                }
                return ReferenceAttribute(name, requireNumber: true);
            }

            public override EmittedExpr VisitNumberParameterReference(CalculateEvaluateParser.NumberParameterReferenceContext context)
            {
                var name = context.IDENTIFIER().GetText();
                return ReferenceAttribute(name, requireNumber: true);
            }

            private EmittedExpr ReferenceAttribute(string name, bool requireNumber)
            {
                if (!_attributes.TryGetValue(name, out var attr))
                    throw new CalculateEvaluateCompilationException($"Unknown reference \"{name}\".");

                bool isNumber = attr.Type == "NUMBER";
                if (requireNumber && !isNumber)
                    throw new CalculateEvaluateCompilationException($"Parameter \"{name}\" is not a number.");

                ReferencedAttributes[attr.Name] = attr;
                return isNumber
                    ? new EmittedExpr($"ca.[{attr.Name}_n]", ExprType.Number)
                    : new EmittedExpr($"ca.[{attr.Name}_t]", ExprType.Text);
            }

            public override EmittedExpr VisitAdditionOrSubtraction(CalculateEvaluateParser.AdditionOrSubtractionContext context)
            {
                var op = context.operation.Type == CalculateEvaluateLexer.PLUS ? "+" : "-";
                return EmitArithmetic(Visit(context.left), Visit(context.right), op);
            }

            public override EmittedExpr VisitMultiplicationOrDivision(CalculateEvaluateParser.MultiplicationOrDivisionContext context)
            {
                var op = context.operation.Type == CalculateEvaluateLexer.TIMES ? "*" : "/";
                return EmitArithmetic(Visit(context.left), Visit(context.right), op);
            }

            public override EmittedExpr VisitNegation(CalculateEvaluateParser.NegationContext context)
            {
                var operand = Visit(context.calculation());
                if (operand.Type != ExprType.Number)
                    throw new CalculateEvaluateCompilationException("Negation requires a numeric operand.");
                return new EmittedExpr($"(-{operand.Sql})", ExprType.Number);
            }

            public override EmittedExpr VisitCalculationGrouping(CalculateEvaluateParser.CalculationGroupingContext context)
            {
                var inner = Visit(context.calculation());
                return new EmittedExpr($"({inner.Sql})", inner.Type);
            }

            public override EmittedExpr VisitInvocation(CalculateEvaluateParser.InvocationContext context)
            {
                var name = context.IDENTIFIER().GetText();
                throw new CalculateEvaluateCompilationException(
                    $"Function \"{name}\" is not supported in viewer criteria. Functions are only available in the equation editor.");
            }

            private EmittedExpr EmitArithmetic(EmittedExpr left, EmittedExpr right, string op)
            {
                left = ResolveArithmeticOperand(left);
                right = ResolveArithmeticOperand(right);
                return new EmittedExpr($"({left.Sql} {op} {right.Sql})", ExprType.Number);
            }

            private EmittedExpr ResolveArithmeticOperand(EmittedExpr expr)
            {
                if (expr.IsLiteral) return MaterializeLiteral(expr, ExprType.Number);
                if (expr.Type != ExprType.Number)
                    throw new CalculateEvaluateCompilationException("Arithmetic operands must be numeric.");
                return expr;
            }
        }

        private sealed class ThrowingErrorListener<TSymbol> : IAntlrErrorListener<TSymbol>
        {
            public static readonly ThrowingErrorListener<TSymbol> Instance = new();
            public void SyntaxError(System.IO.TextWriter output, IRecognizer recognizer, TSymbol offendingSymbol,
                int line, int charPositionInLine, string msg, RecognitionException e) =>
                throw new CalculateEvaluateParsingException(
                    $"Syntax error at line {line}, column {charPositionInLine}: {msg}", e);
        }
    }
}
