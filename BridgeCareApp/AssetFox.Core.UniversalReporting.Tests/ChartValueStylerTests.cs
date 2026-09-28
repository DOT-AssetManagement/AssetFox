using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AssetFox.Core.UniversalReporting.Charts;
using Xunit;

namespace AssetFox.Core.UniversalReporting.Tests
{
    public class ChartValueStylerTests
    {
        [Fact]
        public void Order_Listed_PutsListedValuesFirstThenOthersAToZWithNoneLast()
        {
            var styling = Styling("listed", ("Good", "#2E7D32"), ("Fair", "#F9A825"), ("Poor", "#C62828"));

            var ordered = ChartValueStyler.Order(new[] { "Poor", "(None)", "Fair", "Unrated", "Good", "Closed" }, styling);

            Assert.Equal(new[] { "Good", "Fair", "Poor", "Closed", "Unrated", "(None)" }, ordered);
        }

        [Fact]
        public void Order_Listed_MatchesValuesIgnoringCaseAndSurroundingSpaces()
        {
            var styling = Styling("listed", (" good ", "#2E7D32"), ("FAIR", "#F9A825"));

            var ordered = ChartValueStyler.Order(new[] { "Fair", "Good" }, styling);

            Assert.Equal(new[] { "Good", "Fair" }, ordered);
        }

        [Fact]
        public void Order_Listed_SkipsListedValuesMissingFromTheData()
        {
            var styling = Styling("listed", ("Good", "#2E7D32"), ("Fair", "#F9A825"), ("Poor", "#C62828"));

            var ordered = ChartValueStyler.Order(new[] { "Poor", "Good" }, styling);

            Assert.Equal(new[] { "Good", "Poor" }, ordered);
        }

        [Fact]
        public void Order_Alphabetical_SortsNumbersNaturally()
        {
            var ordered = ChartValueStyler.Order(new[] { "District 10", "District 2", "district 1" }, Styling("alphabetical"));

            Assert.Equal(new[] { "district 1", "District 2", "District 10" }, ordered);
        }

        [Fact]
        public void Order_AlphabeticalDesc_KeepsNoneLast()
        {
            var ordered = ChartValueStyler.Order(new[] { "B", "(None)", "A", "C" }, Styling("alphabeticalDesc"));

            Assert.Equal(new[] { "C", "B", "A", "(None)" }, ordered);
        }

        [Fact]
        public void Order_ValueDesc_SortsByTotalThenAToZ()
        {
            var totals = new Dictionary<string, double> { ["Alpha"] = 5, ["Bravo"] = 10, ["Charlie"] = 5 };

            var ordered = ChartValueStyler.Order(new[] { "Charlie", "Alpha", "Bravo" }, Styling("valueDesc"), name => totals[name]);

            Assert.Equal(new[] { "Bravo", "Alpha", "Charlie" }, ordered);
        }

        [Fact]
        public void Order_WithoutStyling_KeepsDataOrder()
        {
            var ordered = ChartValueStyler.Order(new[] { "Poor", "Fair", "Good" }, null);

            Assert.Equal(new[] { "Poor", "Fair", "Good" }, ordered);
        }

        [Fact]
        public void ActiveStyling_StylesSeriesWhenBoundOtherwiseCategory()
        {
            var config = Config(series: CalculatedFieldBinding("condition-class"));
            config.ValueStyling = Styling("listed", ("Good", "#2E7D32"));
            config.ValueStyling.BindingKey = "calculatedField:condition-class";

            Assert.Equal("series", ChartValueStyler.StyledRole(config));
            Assert.Same(config.ValueStyling, ChartValueStyler.ActiveStyling(config));

            config.Bindings.Series = null;

            Assert.Equal("category", ChartValueStyler.StyledRole(config));
            Assert.Null(ChartValueStyler.ActiveStyling(config));
        }

        [Fact]
        public void ActiveStyling_ForADifferentField_IsIgnored()
        {
            var config = Config(series: CalculatedFieldBinding("condition-class"));
            config.ValueStyling = Styling("listed", ("Good", "#2E7D32"));
            config.ValueStyling.BindingKey = "attribute:DISTRICT";

            Assert.Null(ChartValueStyler.ActiveStyling(config));
        }

        [Fact]
        public void ResolveColors_UsesListedColorsAndGivesOthersUnusedPaletteColors()
        {
            var palette = new[] { Color.FromArgb(0x11, 0x11, 0x11), Color.FromArgb(0x22, 0x22, 0x22), Color.FromArgb(0x33, 0x33, 0x33) };
            var styling = Styling("listed", ("Good", "#222222"), ("Poor", "#C62828"));

            var colors = ChartValueStyler.ResolveColors(new[] { "Good", "Unrated", "Poor", "Closed" }, styling, palette);

            Assert.Equal(
                new[] { "222222", "111111", "C62828", "333333" },
                colors.Select(color => $"{color.R:X2}{color.G:X2}{color.B:X2}"));
        }

        [Fact]
        public void ResolveColors_WhenListedColorsUseTheWholePalette_ReusesThePalette()
        {
            var palette = new[] { Color.FromArgb(0x11, 0x11, 0x11) };
            var styling = Styling("listed", ("Good", "#111111"));

            var colors = ChartValueStyler.ResolveColors(new[] { "Good", "Other" }, styling, palette);

            Assert.Equal(palette[0].ToArgb(), colors[1].ToArgb());
        }

        private static ChartValueStylingDefinition Styling(string order, params (string Value, string Color)[] values) =>
            new()
            {
                BindingKey = "simulationYear:year",
                Order = order,
                Values = values.Select(style => new ChartValueStyleDefinition { Value = style.Value, Color = style.Color }).ToList()
            };

        private static ChartConfigDefinition Config(ChartFieldBindingDefinition? series = null) =>
            new()
            {
                Bindings = new ChartBindingsDefinition
                {
                    Category = new ChartFieldBindingDefinition { Source = "simulationYear", Field = "year", Label = "Year", Role = "dimension" },
                    Value = new ChartFieldBindingDefinition { Source = "assetDetail", Field = "assetCount", Label = "Asset Count", Role = "measure" },
                    Series = series
                }
            };

        private static ChartFieldBindingDefinition CalculatedFieldBinding(string id) =>
            new() { Source = "calculatedField", Field = id, Label = "Condition Class", Role = "series" };
    }
}
