using System;
using System.Collections.Generic;

namespace AssetFox.Core.Reporting.Models.FlexibleReport
{
    public class UniversalReportModel
    {
        public Guid SchemaId { get; set; }
        public string ReportName { get; set; }
        public string SimulationName { get; set; }
        public Guid SimulationId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<ReportSectionModel> Sections { get; set; } = new();
    }

    public class ReportSectionModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; }
        public ReportTableData TableData { get; set; }
        public ReportChartData ChartData { get; set; }
        public ReportAggregationData AggregationData { get; set; }
        public ReportHeaderData HeaderData { get; set; }
        public ReportTextData TextData { get; set; }
    }

    public class ReportTableData
    {
        public List<ReportColumnDefinition> Columns { get; set; } = new();
        public List<List<object>> Rows { get; set; } = new();
        public int TotalRowCount { get; set; }
    }

    public class ReportColumnDefinition
    {
        public string Header { get; set; }
        public string DataPath { get; set; }
        public string Format { get; set; }
        public double Width { get; set; }
    }

    public class ReportChartData
    {
        public string ChartType { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<ChartSeriesData> Series { get; set; } = new();
        public int Height { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowTooltip { get; set; }
    }

    public class ChartSeriesData
    {
        public string Name { get; set; }
        public List<double> Values { get; set; } = new();
        public string Color { get; set; }
    }

    public class ReportAggregationData
    {
        public List<ReportColumnDefinition> Columns { get; set; } = new();
        public List<List<object>> Rows { get; set; } = new();
    }

    public class ReportHeaderData
    {
        public string Text { get; set; }
        public int Level { get; set; } = 1;
    }

    public class ReportTextData
    {
        public string Content { get; set; }
        public string Style { get; set; }
    }
}
