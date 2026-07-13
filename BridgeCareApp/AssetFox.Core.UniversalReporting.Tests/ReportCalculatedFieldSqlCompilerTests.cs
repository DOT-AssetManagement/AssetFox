using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.CalculateEvaluate;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace AssetFox.Core.UniversalReporting.Tests;

public class ReportCalculatedFieldSqlCompilerTests
{
    [Fact]
    public void BuildProjection_Formula_UsesStableReferenceAndSafeDivision()
    {
        var iri = Attribute("IRI", "NUMBER");
        var field = Formula("IRI Half", "[IRI] / 2", iri);
        var parameters = new List<SqlParameter>();

        var plan = new ReportCalculatedFieldSqlCompiler().BuildProjection(
            new[] { field },
            new[] { field.Id },
            Repository(iri).Object,
            ReportCalculatedFieldRowScope.Yearly,
            parameters);

        Assert.Single(plan.Fields);
        Assert.Contains("NULLIF", plan.CteSql);
        Assert.Contains("COALESCE(cfd.[CfAttr0], cfs.[CfAttr0])", plan.CteSql);
        Assert.Contains("LEFT JOIN cfCalculatedFieldDetailAttrs cfd", plan.CteSql);
        Assert.Contains("LEFT JOIN cfCalculatedFieldSummaryAttrs cfs", plan.CteSql);
        Assert.DoesNotContain("{ctePrefix}", plan.CteSql);
        Assert.Contains(iri.Id, parameters.Select(parameter => parameter.Value));
    }

    [Fact]
    public void BuildProjection_Rules_PreserveOrderAndParameterizeText()
    {
        var district = Attribute("District", "STRING");
        var field = new ReportCalculatedField
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Area",
            Mode = "rules",
            ResultType = "text",
            Rules = new List<ReportCalculationRule>
            {
                Rule("equals", "North", "Priority", district),
                Rule("equals", "South", "Standard", district)
            },
            DefaultResult = new ReportCalculationResult { Value = "Other" },
            MissingResult = new ReportCalculationResult { Value = "Unknown" }
        };
        var parameters = new List<SqlParameter>();

        var plan = new ReportCalculatedFieldSqlCompiler().BuildProjection(
            new[] { field },
            new[] { field.Id },
            Repository(district).Object,
            ReportCalculatedFieldRowScope.Summary,
            parameters);

        var north = parameters.Single(parameter => Equals(parameter.Value, "North"));
        var south = parameters.Single(parameter => Equals(parameter.Value, "South"));
        Assert.True(plan.CteSql.IndexOf(north.ParameterName, StringComparison.Ordinal)
            < plan.CteSql.IndexOf(south.ParameterName, StringComparison.Ordinal));
        Assert.Contains(parameters, parameter => Equals(parameter.Value, "Priority"));
        Assert.Contains(parameters, parameter => Equals(parameter.Value, "Unknown"));
        Assert.DoesNotContain("N'North'", plan.CteSql);
    }

    [Fact]
    public void BuildProjection_UnsupportedTextOperator_Throws()
    {
        var district = Attribute("District", "STRING");
        var field = new ReportCalculatedField
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Invalid",
            Mode = "rules",
            ResultType = "text",
            Rules = new List<ReportCalculationRule> { Rule("lessThan", "North", "Bad", district) }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ReportCalculatedFieldSqlCompiler().BuildProjection(
                new[] { field },
                new[] { field.Id },
                Repository(district).Object,
                ReportCalculatedFieldRowScope.Yearly,
                new List<SqlParameter>()));

        Assert.Contains("not supported for text", exception.Message);
    }

    [Fact]
    public void BuildProjection_Rules_CombineStructuredConditionsUsingSelectedMatchMode()
    {
        var district = Attribute("District", "STRING");
        var iri = Attribute("IRI", "NUMBER");
        var field = new ReportCalculatedField
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Attention Needed",
            Mode = "rules",
            ResultType = "text",
            Rules = new List<ReportCalculationRule>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    LogicalOperator = "any",
                    Conditions = new List<ReportRuleCondition>
                    {
                        Condition("equals", "North", district),
                        Condition("greaterThan", "170", iri)
                    },
                    Result = new ReportCalculationResult { Value = "Review" }
                }
            },
            DefaultResult = new ReportCalculationResult { Value = "No" }
        };
        var parameters = new List<SqlParameter>();

        var plan = new ReportCalculatedFieldSqlCompiler().BuildProjection(
            new[] { field },
            new[] { field.Id },
            Repository(district, iri).Object,
            ReportCalculatedFieldRowScope.Yearly,
            parameters);

        Assert.Contains(" OR ", plan.CteSql);
        Assert.Contains(parameters, parameter => Equals(parameter.Value, "North"));
        Assert.Contains(parameters, parameter => Equals(parameter.Value, 170d));
    }

    [Fact]
    public void BuildProjection_CustomNamespaces_CanShareOneQuery()
    {
        var iri = Attribute("IRI", "NUMBER");
        var field = Formula("IRI Copy", "[IRI]", iri);
        var parameters = new List<SqlParameter>();
        var compiler = new ReportCalculatedFieldSqlCompiler();

        var initial = compiler.BuildProjection(
            new[] { field }, new[] { field.Id }, Repository(iri).Object,
            ReportCalculatedFieldRowScope.SummaryByMaintainableAsset, parameters, "initial");
        var yearly = compiler.BuildProjection(
            new[] { field }, new[] { field.Id }, Repository(iri).Object,
            ReportCalculatedFieldRowScope.Yearly, parameters, "yearly");

        Assert.NotEqual(initial.ValuesCteName, yearly.ValuesCteName);
        Assert.Contains("MaintainableAssetId AS Id", initial.CteSql);
        Assert.Contains(parameters, parameter => parameter.ParameterName == "@initial_attr_0");
        Assert.Contains(parameters, parameter => parameter.ParameterName == "@yearly_attr_0");
    }

    [Fact]
    public void ValidateSchemaReferences_DanglingComponentField_Throws()
    {
        var schema = new ReportSchemaDefinition
        {
            Pages = new List<ReportPage>
            {
                new()
                {
                    Components = new List<ReportComponent>
                    {
                        new()
                        {
                            Title = "Assets",
                            Config = new Dictionary<string, object>
                            {
                                ["attributeColumns"] = new[]
                                {
                                    new { source = "calculatedField", fieldId = Guid.NewGuid().ToString() }
                                }
                            }
                        }
                    }
                }
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ReportCalculatedFieldSqlCompiler().ValidateSchemaReferences(schema));

        Assert.Contains("not defined", exception.Message);
    }

    private static AttributeDTO Attribute(string name, string type) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Type = type
    };

    private static ReportCalculatedField Formula(string name, string expression, AttributeDTO attribute) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        Mode = "formula",
        ResultType = "number",
        Formula = Expression(expression, attribute)
    };

    private static ReportCalculationRule Rule(string comparisonOperator, string value, string result, AttributeDTO attribute) => new()
    {
        Id = Guid.NewGuid().ToString(),
        LogicalOperator = "all",
        Conditions = new List<ReportRuleCondition> { Condition(comparisonOperator, value, attribute) },
        Result = new ReportCalculationResult { Value = result }
    };

    private static ReportRuleCondition Condition(string comparisonOperator, string value, AttributeDTO attribute) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Attribute = new ReportAttributeReference
        {
            AttributeId = attribute.Id.ToString(),
            Name = attribute.Name,
            Token = attribute.Name
        },
        Operator = comparisonOperator,
        Value = value
    };

    private static ReportExpressionDefinition Expression(string expression, AttributeDTO attribute) => new()
    {
        Expression = expression,
        AttributeReferences = new List<ReportAttributeReference>
        {
            new()
            {
                AttributeId = attribute.Id.ToString(),
                Name = attribute.Name,
                Token = attribute.Name
            }
        }
    };

    private static Mock<IAttributeRepository> Repository(params AttributeDTO[] attributes)
    {
        var repository = new Mock<IAttributeRepository>();
        repository.Setup(item => item.GetSingleById(It.IsAny<Guid>()))
            .Returns<Guid>(id => attributes.FirstOrDefault(attribute => attribute.Id == id)!);
        repository.Setup(item => item.GetSingleByName(It.IsAny<string>()))
            .Returns<string>(name => attributes.FirstOrDefault(attribute =>
                string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))!);
        return repository;
    }
}
