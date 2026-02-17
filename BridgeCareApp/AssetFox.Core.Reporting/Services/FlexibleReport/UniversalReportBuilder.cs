using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Reporting.Models.FlexibleReport;

namespace AssetFox.Core.Reporting.Services.FlexibleReport
{
    public class UniversalReportBuilder
    {
        private readonly DataExtractor _dataExtractor;

        public UniversalReportBuilder()
        {
            _dataExtractor = new DataExtractor();
        }

        public UniversalReportModel Build(
            ReportSchemaDefinition schema,
            SimulationOutput simulationOutput,
            Guid schemaId,
            Guid simulationId,
            string simulationName)
        {
            var model = new UniversalReportModel
            {
                SchemaId = schemaId,
                ReportName = schema.Name,
                SimulationName = simulationName,
                SimulationId = simulationId,
                GeneratedAt = DateTime.UtcNow
            };

            if (schema.Components == null) return model;

            var orderedComponents = schema.Components.OrderBy(c => c.Order).ToList();

            foreach (var component in orderedComponents)
            {
                var sectionModel = BuildComponentSection(component, simulationOutput);
                model.Sections.Add(sectionModel);
            }

            return model;
        }

        private ReportSectionModel BuildComponentSection(ReportComponent component, SimulationOutput output)
        {
            var section = new ReportSectionModel
            {
                Id = component.Id ?? Guid.NewGuid().ToString(),
                Type = component.Type,
                Title = component.Title,
                Order = component.Order,
                Visible = component.Visible,
                ExcelTabName = component.ExcelTabName
            };

            var config = component.Config ?? new Dictionary<string, object>();

            switch (component.Type?.ToLowerInvariant())
            {
                case "assetdetail":
                    section.TableData = _dataExtractor.AssembleAssetDetailData(output, config);
                    break;

                case "treatmenttimeline":
                    section.TableData = _dataExtractor.AssembleTreatmentTimelineData(output, config);
                    break;

                case "worksummary":
                    section.AggregationData = _dataExtractor.AssembleWorkSummaryData(output, config);
                    break;

                case "budgetsummary":
                    section.TableData = _dataExtractor.AssembleBudgetSummaryData(output, config);
                    break;

                case "conditiontracking":
                    section.TableData = _dataExtractor.AssembleConditionTrackingData(output, config);
                    break;

                case "fundedtreatmentlist":
                    section.TableData = _dataExtractor.AssembleFundedTreatmentListData(output, config);
                    break;

                case "goaltracking":
                    section.TableData = _dataExtractor.AssembleGoalTrackingData(output, config);
                    break;

                case "textblock":
                    section.TextData = _dataExtractor.AssembleTextBlockData(config);
                    break;
            }

            return section;
        }
    }
}
