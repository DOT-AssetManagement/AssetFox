using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.ExcelHelpers;
using AssetFox.Core.Reporting.Models;
using AssetFox.Core.Reporting.Models.PAMSPBExport;
using AssetFox.Core.Reporting.Services.PAMSSummaryReport;
using OfficeOpenXml;

namespace AssetFox.Core.Reporting.Services.PAMSPBExport
{
    public class TreatmentTab
    {
        private ReportHelper _reportHelper;
        private List<string> treatmentAttributes;
        private readonly IUnitOfWork _unitOfWork;

        public TreatmentTab(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _reportHelper = new ReportHelper(_unitOfWork);
        }

        public void Fill(ExcelWorksheet treatmentsWorksheet, SimulationOutput simulationOutput, Guid simulationId, Guid networkId, List<TreatmentDTO> scenarioSelectableTreatmentsDtos, List<MaintainableAsset> networkMaintainableAssets, bool shouldBundleFeasibleTreatments, List<DTOs.Abstract.BaseCommittedProjectDTO> committedProjectList, string primaryKey)
        {
            var currentCell = AddHeadersCells(treatmentsWorksheet);

            FillDynamicDataInWorkSheet(simulationOutput, treatmentsWorksheet, currentCell, simulationId, networkId, scenarioSelectableTreatmentsDtos, networkMaintainableAssets, shouldBundleFeasibleTreatments, committedProjectList, primaryKey);            
            treatmentsWorksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
            treatmentsWorksheet.Cells.AutoFitColumns();
        }

        private void FillDynamicDataInWorkSheet(SimulationOutput simulationOutput, ExcelWorksheet treatmentsWorksheet, CurrentCell currentCell, Guid simulationId, Guid networkId, List<TreatmentDTO> scenarioSelectableTreatmentsDtos, List<MaintainableAsset> networkMaintainableAssets, bool shouldBundleFeasibleTreatments, List<DTOs.Abstract.BaseCommittedProjectDTO> committedProjectList, string primaryKey)
        {
            foreach (var initialAssetSummary in simulationOutput.InitialAssetSummaries)
            {
                var assetId = initialAssetSummary.AssetId;
                var orderedYears = simulationOutput.Years.OrderBy(yr => yr.Year);
                Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails = new();
                foreach (var year in orderedYears)
                {
                    var section = year.Assets.FirstOrDefault(_ => _.AssetId == assetId);
                    if (section.TreatmentCause == TreatmentCause.NoSelection || section.TreatmentCause == TreatmentCause.Undefined)
                    {
                        continue;
                    }

                    // Generate data model                    
                    var treatmentDataModel = GenerateTreatmentDataModel(assetId, year, section, simulationId, networkId, scenarioSelectableTreatmentsDtos, networkMaintainableAssets, keyCashFlowFundingDetails, shouldBundleFeasibleTreatments, committedProjectList, primaryKey, initialAssetSummary);

                    // Fill in excel
                    currentCell = FillDataInWorksheet(treatmentsWorksheet, treatmentDataModel, currentCell);
                }
                keyCashFlowFundingDetails.Clear();
            }
            ExcelHelper.ApplyBorder(treatmentsWorksheet.Cells[1, 1, currentCell.Row - 1, currentCell.Column]);
        }

        private CurrentCell FillDataInWorksheet(ExcelWorksheet treatmentsWorksheet, TreatmentDataModel treatmentDataModel, CurrentCell currentCell)
        {
            var row = currentCell.Row;
            int column = 1;

            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.SimulationId;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.NetworkId;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.MaintainableAssetId;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.AssetName;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.District;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Cnty;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Route;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Direction;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Year;
            treatmentsWorksheet.Cells[row, column++].Value = string.Empty;
            treatmentsWorksheet.Cells[row, column++].Value = string.Empty;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Appliedtreatment;
            SetDecimalFormat(treatmentsWorksheet.Cells[row, column]);
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Cost;
            SetDecimalFormat(treatmentsWorksheet.Cells[row, column]);
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Benefit;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Category;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Budget;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Bpn;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.FunctionalClass;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.NhsInd;

            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.CRS;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.FromSection;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.ToSection;
            
            SetDecimalFormat(treatmentsWorksheet.Cells[row, column]);
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.RiskScore;
            SetDecimalFormat(treatmentsWorksheet.Cells[row, column]);
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.RemainingLife;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.PriorityLevel;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.TreatmentFundingIgnoresSpendingLimit;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.TreatmentStatus;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.TreatmentCause;            
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.ProjectId;
            treatmentsWorksheet.Cells[row, column++].Value = treatmentDataModel.Interstate;

            foreach (var treatmentAttributeValue in treatmentDataModel.TreatmentAttributeValues)
            {
                SetDecimalFormat(treatmentsWorksheet.Cells[row, column]);
                treatmentsWorksheet.Cells[row, column++].Value = treatmentAttributeValue;
            }

            return new CurrentCell { Row = ++row, Column = column - 1 };
        }

        private static void SetDecimalFormat(ExcelRange cell) => ExcelHelper.SetCustomFormat(cell, ExcelHelperCellFormat.DecimalPrecision3);

        private TreatmentDataModel GenerateTreatmentDataModel(Guid assetId, SimulationYearDetail year, AssetDetail section, Guid simulationId, Guid networkId, List<TreatmentDTO> scenarioSelectableTreatmentsDtos, List<MaintainableAsset> networkMaintainableAssets, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, bool shouldBundleFeasibleTreatments, List<DTOs.Abstract.BaseCommittedProjectDTO> committedProjectList, string primaryKey, AssetSummaryDetail initialAssetSummary)
        {
            var appliedTreatment = section.AppliedTreatment;
            TreatmentDataModel treatmentDataModel = new TreatmentDataModel
            {
                SimulationId = simulationId,
                NetworkId = networkId,
                MaintainableAssetId = assetId,
                Year = year.Year,
                Appliedtreatment = appliedTreatment,
            };

            var locationIdentifier = networkMaintainableAssets.FirstOrDefault(_ => _.Id == assetId)?.Location?.LocationIdentifier;
            treatmentDataModel.AssetName = locationIdentifier;
            var fromSection = string.Empty;
            var toSection = string.Empty;
            var direction = string.Empty;
            if (!string.IsNullOrEmpty(locationIdentifier))
            {
                var parts = locationIdentifier.Split(['_']);
                direction = parts[2];
                var fromTo = parts.Last()?.Split('-');
                fromSection = fromTo?.First();
                toSection = fromTo?.Last();
            }
            treatmentDataModel.FromSection = fromSection;
            treatmentDataModel.ToSection = toSection;

            var valuePerTextAttribute = section.ValuePerTextAttribute;
            var initialSummaryValuePerTextAttribute = initialAssetSummary.ValuePerTextAttribute;
            var initialSummaryValuePerNumericAttribute = initialAssetSummary.ValuePerNumericAttribute;
            treatmentDataModel.District = CheckGetTextValue(initialSummaryValuePerTextAttribute, "DISTRICT");
            treatmentDataModel.Cnty = CheckGetTextValue(initialSummaryValuePerTextAttribute, "CNTY");
            treatmentDataModel.Route = CheckGetTextValue(initialSummaryValuePerTextAttribute, "SR");
            treatmentDataModel.CRS = CheckGetTextValue(initialSummaryValuePerTextAttribute, "CRS");
            treatmentDataModel.Direction = direction;
            treatmentDataModel.RiskScore = CheckGetNumericValue(initialSummaryValuePerNumericAttribute, "RISKSCORE");
            treatmentDataModel.Interstate = CheckGetTextValue(initialSummaryValuePerTextAttribute, "INTERSTATE");                        
            treatmentDataModel.Bpn = CheckGetTextValue(initialSummaryValuePerTextAttribute, "BUSIPLAN");
            treatmentDataModel.FunctionalClass = CheckGetNumericValue(initialSummaryValuePerNumericAttribute, "F_CLASS");
            treatmentDataModel.NhsInd = CheckGetTextValue(initialSummaryValuePerTextAttribute, "NHS_IND");                        

            var treatmentOption = section.TreatmentOptions.FirstOrDefault(_ => _.TreatmentName == appliedTreatment);            
            treatmentDataModel.Benefit = treatmentOption != null ? treatmentOption.Benefit : 0;
            // TODO remove infinity condition once fix is available for such edge cases
            treatmentDataModel.RemainingLife = treatmentOption != null && treatmentOption.RemainingLife?.ToString() != "-8" ? treatmentOption.RemainingLife : 0;

            var primaryKeyValue = CheckGetTextValue(section.ValuePerTextAttribute, primaryKey);

            // Build keyCashFlowFundingDetails
            _reportHelper.BuildKeyCashFlowFundingDetails(year, section, primaryKeyValue, keyCashFlowFundingDetails);

            // If CF then use obj from keyCashFlowFundingDetails otherwise from section
            var treatmentConsiderations = ((section.TreatmentCause == TreatmentCause.SelectedTreatment &&
                                          section.TreatmentStatus == TreatmentStatus.Progressed) ||
                                          (section.TreatmentCause == TreatmentCause.CashFlowProject &&
                                          section.TreatmentStatus == TreatmentStatus.Progressed) ||
                                          (section.TreatmentCause == TreatmentCause.CashFlowProject &&
                                          section.TreatmentStatus == TreatmentStatus.Applied)) ?
                                          keyCashFlowFundingDetails[primaryKeyValue] :
                                          section.TreatmentConsiderations ?? [];

            var treatmentConsideration = shouldBundleFeasibleTreatments ?
                                         treatmentConsiderations.FirstOrDefault(_ => _.FundingCalculationOutput != null &&
                                            _.FundingCalculationOutput.AllocationMatrix.Any(_ => _.Year == year.Year) &&
                                            section.AppliedTreatment.Contains(_.TreatmentName)) :
                                         treatmentConsiderations.FirstOrDefault(_ => _.FundingCalculationOutput != null &&
                                            _.FundingCalculationOutput.AllocationMatrix.Any(_ => _.Year == year.Year) &&
                                            _.TreatmentName == section.AppliedTreatment);

            var cost = treatmentConsideration == null ? 0
                : Math.Round(treatmentConsideration.FundingCalculationOutput?
                            .AllocationMatrix.Where(_ => _.Year == year.Year)?.Sum(b => b.AllocatedAmount)
                            ?? 0, 0);
            treatmentDataModel.Cost = cost;

            treatmentDataModel.PriorityLevel = treatmentConsideration?.BudgetPriorityLevel;
            treatmentDataModel.TreatmentFundingIgnoresSpendingLimit = section.TreatmentFundingIgnoresSpendingLimit ? 1 : 0;
            treatmentDataModel.TreatmentCause = section.TreatmentCause.ToString();
            treatmentDataModel.TreatmentStatus = section.TreatmentStatus.ToString();
          
            var budgetsUsed = treatmentConsideration?.FundingCalculationOutput?.AllocationMatrix?.Where(_ => _.AllocatedAmount > 0).Select(_ =>
                              _.BudgetName).Distinct().ToList() ?? new();
            treatmentDataModel.Budget = string.Join(", ", budgetsUsed);
            treatmentDataModel.Category = shouldBundleFeasibleTreatments && appliedTreatment.Contains("Bundle") ? PAMSConstants.Bundled : scenarioSelectableTreatmentsDtos.FirstOrDefault(_ => _.Name == appliedTreatment)?.Category.ToString();

            // Project Id
            var committedProject = committedProjectList.FirstOrDefault(_ => _.Treatment.All(_ => appliedTreatment.Contains(_))
                                    && _.Year == year.Year
                                    && _.LocationKeys[primaryKey] == primaryKeyValue);
            treatmentDataModel.ProjectId = committedProject?.ProjectId?.ToString() ?? string.Empty;

            // Consequences
            var treatmentAttributeValues = new List<double>();
            foreach (var treatmentAttribute in treatmentAttributes)
            {
                var treatmentAttributeValue = section.ValuePerNumericAttribute.ContainsKey(treatmentAttribute)
                    ? CheckGetNumericValue(section.ValuePerNumericAttribute, treatmentAttribute)
                    : CheckGetNumericValue(initialSummaryValuePerNumericAttribute, treatmentAttribute);
                treatmentAttributeValues.Add(treatmentAttributeValue);
            }
            treatmentDataModel.TreatmentAttributeValues = treatmentAttributeValues;

            return treatmentDataModel;
        }

        private double CheckGetNumericValue(IDictionary<string, double> valuePerNumericAttribute, string attribute) => _reportHelper.CheckAndGetValue<double>(valuePerNumericAttribute, attribute);

        private string CheckGetTextValue(IDictionary<string, string> valuePerTextAttribute, string attribute) =>
          _reportHelper.CheckAndGetValue<string>(valuePerTextAttribute, attribute);

        private CurrentCell AddHeadersCells(ExcelWorksheet worksheet)
        {
            int headerRow = 1;
            int column = 1;

            worksheet.Cells.Style.WrapText = false;

            worksheet.Cells[headerRow, column++].Value = "SimulationID";
            worksheet.Cells[headerRow, column++].Value = "NetworkID";
            worksheet.Cells[headerRow, column++].Value = "AssetId";
            worksheet.Cells[headerRow, column++].Value = "AssetName";
            worksheet.Cells[headerRow, column++].Value = "District";
            worksheet.Cells[headerRow, column++].Value = "County";
            worksheet.Cells[headerRow, column++].Value = "Route";
            worksheet.Cells[headerRow, column++].Value = "Direction";
            worksheet.Cells[headerRow, column++].Value = "Year";
            worksheet.Cells[headerRow, column++].Value = "MinYear";
            worksheet.Cells[headerRow, column++].Value = "MaxYear";
            worksheet.Cells[headerRow, column++].Value = "Treatment";
            worksheet.Cells[headerRow, column++].Value = "Cost";
            worksheet.Cells[headerRow, column++].Value = "Benefit";
            worksheet.Cells[headerRow, column++].Value = "Category";
            worksheet.Cells[headerRow, column++].Value = "Budget";
            worksheet.Cells[headerRow, column++].Value = "BPN";
            worksheet.Cells[headerRow, column++].Value = "FunctionalClass";
            worksheet.Cells[headerRow, column++].Value = "NHS";
            
            worksheet.Cells[headerRow, column++].Value = "CRS";
            worksheet.Cells[headerRow, column++].Value = "FromSection";
            worksheet.Cells[headerRow, column++].Value = "ToSection";

            worksheet.Cells[headerRow, column++].Value = "Risk";
            worksheet.Cells[headerRow, column++].Value = "RemainingLife";
            worksheet.Cells[headerRow, column++].Value = "PriorityOrder";
            worksheet.Cells[headerRow, column++].Value = "TreatmentFundingIgnoresSpendingLimit";
            worksheet.Cells[headerRow, column++].Value = "TreatmentStatus";
            worksheet.Cells[headerRow, column++].Value = "TreatmentCause";
            worksheet.Cells[headerRow, column++].Value = "Project Id";
            worksheet.Cells[headerRow, column++].Value = "Interstate";
            
            treatmentAttributes = GetTreatmentAttributes();
            foreach(var treatmentAttribute in treatmentAttributes)
            {
                worksheet.Cells[headerRow, column++].Value = treatmentAttribute;
            }

            return new CurrentCell { Row = ++headerRow, Column = worksheet.Dimension.Columns + 1 };
        }

        private static List<string> GetTreatmentAttributes() =>
        [
            "SURFACEID","LAST_STRUCTURAL_OVERLAY","YEAR_LAST_OVERLAY","ROUGHNESS","AGE","YR_BUILT","OPI","CNSLABCT","CJOINTCT","CPCCPACT","CPCCPASF",
            "BPATCHCT","BPATCHSF","CBPATCCT","CBPATCSF","BMISCCK1","BMISCCK2","BMISCCK3","BLRUTDP1","BLRUTDP2","BLRUTDP3","BRRUTDP1","BRRUTDP2","BRRUTDP3",
            "BEDGDTR1","BEDGDTR2","BEDGDTR3","BLTEDGE1","BLTEDGE2","BLTEDGE3","BFATICR1","BFATICR2","BFATICR3","BTRNSFT1","BTRNSFT2","BTRNSFT3","BTRNSCT1",
            "BTRNSCT2","BTRNSCT3","BRAVLWT2","BRAVLWT3","CLNGCRK1","CLNGCRK2","CLNGCRK3","CTRNCRK1","CTRNCRK2","CTRNCRK3","CLJCPRU1","CLJCPRU2","CLJCPRU3",
            "CRJCPRU1","CRJCPRU2","CRJCPRU3","CLNGJNT1","CLNGJNT2","CLNGJNT3","CTRNJNT1","CTRNJNT2","CTRNJNT3","CFLTJNT2","CFLTJNT3","CBRKSLB1","CBRKSLB2","CBRKSLB3"
        ];
    }
}
