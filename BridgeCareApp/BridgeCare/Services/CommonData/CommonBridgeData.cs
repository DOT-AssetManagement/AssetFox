using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BridgeCare.Interfaces;
using BridgeCare.Interfaces.SummaryReport;
using BridgeCare.Models;
using BridgeCare.Models.CommonReportData;
using BridgeCare.Models.SummaryReport.ParametersTAB;
using BridgeCare.Services.SummaryReport.BridgeData;

namespace BridgeCare.Services.CommonData
{
    public class CommonBridgeData
    {
        private readonly IBridgeData bridgeData;
        private readonly BridgeDataHelper bridgeDataHelper;
        private readonly ParametersModel parametersModel;

        public CommonBridgeData(IBridgeData bridgeData, BridgeDataHelper bridgeDataHelper, ParametersModel parametersModel)
        {
            this.bridgeData = bridgeData;
            this.bridgeDataHelper = bridgeDataHelper;
            this.parametersModel = parametersModel;
        }
        public CommonReportDataModel Get(SimulationModel simulationModel, List<int> simulationYears, BridgeCareContext dbContext)
        {
            var simulationDataTable = bridgeData.GetSimulationData(simulationModel, dbContext, simulationYears);

            var projectCostModels = bridgeData.GetReportData(simulationModel, dbContext, simulationYears);
            var budgetsPerBrKey = bridgeData.GetBudgetsPerBRKey(simulationModel, dbContext);

            var simulationDataModels = bridgeDataHelper.GetSimulationDataModels(simulationDataTable, simulationYears, projectCostModels, budgetsPerBrKey);

            var commonReportDataModel = new CommonReportDataModel
            {
                SimulationDataModels = simulationDataModels,
                BudgetsPerBRKeys = budgetsPerBrKey,
                ParametersModel = parametersModel,
            };
            return commonReportDataModel;
        }
    }
}
