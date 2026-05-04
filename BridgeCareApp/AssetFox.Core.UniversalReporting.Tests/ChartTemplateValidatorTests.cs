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
    }
}
