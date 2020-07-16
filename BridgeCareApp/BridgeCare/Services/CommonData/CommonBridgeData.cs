using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using BridgeCare.Interfaces;
using BridgeCare.Models;
using BridgeCare.Models.CommonReportData;
using BridgeCare.Models.SummaryReport.ParametersTAB;
using BridgeCare.Services.SummaryReport.BridgeData;
using OfficeOpenXml;

namespace BridgeCare.Services.CommonData
{
    public class CommonBridgeData
    {
        private readonly IBridgeData bridgeData;
        private readonly BridgeDataHelper bridgeDataHelper;
        private readonly ExcelHelper excelHelper;
        private readonly ParametersModel parametersModel;

        public CommonBridgeData(IBridgeData bridgeData, BridgeDataHelper bridgeDataHelper, ExcelHelper excelHelper, ParametersModel parametersModel)
        {
            this.bridgeData = bridgeData;
            this.bridgeDataHelper = bridgeDataHelper;
            this.excelHelper = excelHelper;
            this.parametersModel = parametersModel;
        }
        public CommonReportDataModel Get(SimulationModel simulationModel, List<int> simulationYears, BridgeCareContext dbContext)
        {
            //var sections = bridgeData.GetSectionData(simulationModel, dbContext);
            var simulationDataTable = bridgeData.GetSimulationData(simulationModel, dbContext, simulationYears);
            var sectionIdsFromSimulationTable = from dt in simulationDataTable.AsEnumerable()
                                                select dt.Field<int>("SECTIONID");
            //var sectionsForSummaryReport = sections.Where(sm => sectionIdsFromSimulationTable.Contains(sm.SECTIONID)).ToList();

            var projectCostModels = bridgeData.GetReportData(simulationModel, dbContext, simulationYears);
            var budgetsPerBrKey = bridgeData.GetBudgetsPerBRKey(simulationModel, dbContext);

            var simulationDataModels = bridgeDataHelper.GetSimulationDataModels(simulationDataTable, simulationYears, projectCostModels, budgetsPerBrKey);

            var commonReportDataModel = new CommonReportDataModel
            {
                SimulationDataModels = simulationDataModels,
                BudgetsPerBRKeys = budgetsPerBrKey,
                ParametersModel = parametersModel,
                //SectionsForSummaryReport = sectionsForSummaryReport
            };
            return commonReportDataModel;
        }
    }
}
