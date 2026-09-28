using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.UniversalReporting.Charts;
using AssetFox.Core.UniversalReporting.Schema;
using Xunit;

namespace AssetFox.Core.UniversalReporting.Tests
{
    public class ChartTemplateValidatorTests
    {
        [Fact]
        public void ValidateComponent_ValidStackedBar_ReturnsNoBlockers()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "stacked");

            var issues = validator.ValidateComponent(component);

            Assert.DoesNotContain(issues, issue => issue.Blocker);
        }

        [Fact]
        public void ValidateComponent_ValidSingleBarWithoutSeries_ReturnsNoBlockers()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "single", includeSeries: false);

            var issues = validator.ValidateComponent(component);

            Assert.DoesNotContain(issues, issue => issue.Blocker);
        }

        [Fact]
        public void ValidateComponent_MissingValue_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "grouped");
            component.Config["bindings"] = new Dictionary<string, object>
            {
                ["category"] = YearBinding()
            };

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("value field"));
        }

        [Fact]
        public void ValidateComponent_PieWithSeries_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("pieChart", "pie", "pie");

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("series split"));
        }

        [Fact]
        public void ValidateComponent_InvalidTopN_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("pieChart", "pie", "pie", includeSeries: false);
            component.Config["limit"] = new Dictionary<string, object>
            {
                ["topN"] = 101,
                ["includeOther"] = false
            };

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("Top N"));
        }

        [Fact]
        public void Read_MissingFilters_InitializesExcludeNoTreatmentFilter()
        {
            var config = ChartConfigReader.Read(new Dictionary<string, object>());

            Assert.False(config.Filters.ExcludeNoTreatment);
        }

        [Fact]
        public void ValidateComponent_WeightedAverage_ReturnsNoBlockers()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "single", includeSeries: false);
            ((Dictionary<string, object>)component.Config["bindings"])["value"] = AttributeBinding("IRI", "weightedAvg");

            var issues = validator.ValidateComponent(component);

            Assert.DoesNotContain(issues, issue => issue.Blocker);
        }

        [Fact]
        public void ValidateComponent_UnsupportedAggregation_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "single", includeSeries: false);
            ((Dictionary<string, object>)component.Config["bindings"])["value"] = AttributeBinding("IRI", "median");

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("unsupported aggregation"));
        }

        [Fact]
        public void ValidateComponent_ValidValueStyling_ReturnsNoBlockers()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "stacked");
            component.Config["valueStyling"] = ValueStyling("listed", ("Good", "#2E7D32"), ("Fair", "#f9a825"), ("Poor", "#C62828"));

            var issues = validator.ValidateComponent(component);

            Assert.DoesNotContain(issues, issue => issue.Blocker);
        }

        [Fact]
        public void ValidateComponent_ValueStylingWithBadEntries_ReturnsBlockers()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "stacked");
            component.Config["valueStyling"] = ValueStyling("listed", ("Good", "#2E7D32"), ("good ", "#7CB342"), (" ", "#F9A825"), ("Poor", "red"));

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("\"Good\" more than once"));
            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("blank value"));
            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("\"Poor\" has an invalid color"));
        }

        [Fact]
        public void ValidateComponent_ValueStylingWithTooManyValues_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "stacked");
            component.Config["valueStyling"] = ValueStyling(
                "listed",
                Enumerable.Range(1, 31).Select(i => ($"Value {i}", "#2A578D")).ToArray());

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("at most 30 values"));
        }

        [Fact]
        public void ValidateComponent_ValueStylingWithUnknownOrder_ReturnsBlocker()
        {
            var validator = new ChartTemplateValidator();
            var component = CreateChartComponent("barChart", "bar", "stacked");
            component.Config["valueStyling"] = ValueStyling("random");

            var issues = validator.ValidateComponent(component);

            Assert.Contains(issues, issue => issue.Blocker && issue.Message.Contains("unsupported order"));
        }

        [Fact]
        public void Read_ParsedSchemaJson_ReadsValueStyling()
        {
            var schema = SchemaParser.Parse(@"{
                ""pages"": [{ ""components"": [{
                    ""type"": ""barChart"",
                    ""config"": {
                        ""chartKind"": ""bar"",
                        ""valueStyling"": {
                            ""bindingKey"": ""calculatedField:condition-class"",
                            ""order"": ""listed"",
                            ""values"": [{ ""value"": ""Good"", ""color"": ""#2E7D32"" }, { ""value"": ""Poor"", ""color"": ""#C62828"" }]
                        }
                    }
                }] }]
            }");

            var config = ChartConfigReader.Read(schema.AllComponents.Single().Config);

            Assert.Equal("calculatedField:condition-class", config.ValueStyling!.BindingKey);
            Assert.Equal(new[] { "Good", "Poor" }, config.ValueStyling.Values.Select(style => style.Value));
            Assert.Equal("#C62828", config.ValueStyling.Values[1].Color);
        }

        private static Dictionary<string, object> ValueStyling(string order, params (string Value, string Color)[] values) =>
            new()
            {
                ["bindingKey"] = "treatment:treatmentName",
                ["order"] = order,
                ["values"] = values
                    .Select(style => new Dictionary<string, object> { ["value"] = style.Value, ["color"] = style.Color })
                    .ToList()
            };

        private static ReportComponent CreateChartComponent(
            string type,
            string kind,
            string mode,
            bool includeSeries = true) =>
            new()
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Type = type,
                Title = "Chart",
                Config = new Dictionary<string, object>
                {
                    ["chartKind"] = kind,
                    ["mode"] = mode,
                    ["bindings"] = BuildBindings(includeSeries)
                }
            };

        private static Dictionary<string, object> BuildBindings(bool includeSeries)
        {
            var bindings = new Dictionary<string, object>
            {
                ["category"] = YearBinding(),
                ["value"] = AllocatedAmountBinding()
            };

            if (includeSeries)
            {
                bindings["series"] = TreatmentBinding();
            }

            return bindings;
        }

        private static Dictionary<string, object> YearBinding() =>
            new()
            {
                ["source"] = "simulationYear",
                ["field"] = "year",
                ["label"] = "Year",
                ["role"] = "dimension",
                ["format"] = "year"
            };

        private static Dictionary<string, object> TreatmentBinding() =>
            new()
            {
                ["source"] = "treatment",
                ["field"] = "treatmentName",
                ["label"] = "Treatment",
                ["role"] = "series",
                ["format"] = "text"
            };

        private static Dictionary<string, object> AllocatedAmountBinding() =>
            new()
            {
                ["source"] = "allocation",
                ["field"] = "allocatedAmount",
                ["label"] = "Dollars Spent",
                ["role"] = "measure",
                ["aggregate"] = "sum",
                ["format"] = "currency"
            };

        private static Dictionary<string, object> AttributeBinding(string name, string aggregate) =>
            new()
            {
                ["source"] = "attribute",
                ["field"] = name,
                ["label"] = name,
                ["role"] = "measure",
                ["aggregate"] = aggregate,
                ["format"] = "number"
            };
    }
}
