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
        public string Type { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; }
        public ReportTableData TableData { get; set; }
        public ReportAggregationData AggregationData { get; set; }
        public ReportTextData TextData { get; set; }
    }

    public class ReportTableData
    {
        public List<ReportColumnDefinition> Columns { get; set; } = new();
        public List<List<object>> Rows { get; set; } = new();
    }

    public class ReportColumnDefinition
    {
        public string Header { get; set; }
        public string DataPath { get; set; }
        public string Format { get; set; }
        public double Width { get; set; }
    }

    public class ReportAggregationData
    {
        public List<ReportColumnDefinition> Columns { get; set; } = new();
        public List<List<object>> Rows { get; set; } = new();
    }

    public class ReportTextData
    {
        public string Content { get; set; }
        public string Style { get; set; }
    }
}
