using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using OfficeOpenXml;
using AppliedResearchAssociates.iAM.ExcelHelpers;
using System.Drawing;
using CurrentCell = AppliedResearchAssociates.iAM.Reporting.Models.PAMSSummaryReport.CurrentCell;

namespace AppliedResearchAssociates.iAM.Reporting.Services.PAMSSummaryReport.CountySummary
{
    public class DistrictCounty
    {
        public int District { get; set; }
        public string County { get; set; }
    }

    public class DistrictCountyCost: DistrictCounty
    {
        public int Year { get; set; }
        public decimal Cost { get; set; }
        public decimal Percent { get; set; }
        public decimal DistrictSum { get; set; }
    }

    public class CountySummary
    {
        private SummaryReportHelper _summaryReportHelper;
        private IList<DistrictCounty> _uniqueDistrictCountyList = null;
        private IList<DistrictCountyCost> _districtCountyCostList = null;

        private CurrentCell currentCell = null;

        public CountySummary()
        {
            _summaryReportHelper = new SummaryReportHelper();
            if (_summaryReportHelper == null) { throw new ArgumentNullException(nameof(_summaryReportHelper)); }

            //create list object
            _uniqueDistrictCountyList = [];
        }

        public void Fill(ExcelWorksheet worksheet, SimulationOutput reportOutputData, List<int> simulationYears, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, string primaryKey)
        {
            FillDataToUseInExcel(worksheet, reportOutputData, simulationYears, keyCashFlowFundingDetails, primaryKey);
            worksheet.Cells.AutoFitColumns();
        }

        #region Private methods

        private void FillDataToUseInExcel(ExcelWorksheet worksheet, SimulationOutput reportOutputData, List<int> simulationYears, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, string primaryKey)
        {
            //fill unique district county list
            BuildUniqueDistrictCountyList(reportOutputData);

            //get data by district and county and build list
            BuildDistrictCountyCostList(reportOutputData, simulationYears, keyCashFlowFundingDetails, primaryKey);

            //Fill Budget By County
            FillBudgetByCountyInExcel(worksheet, simulationYears);

            //Fill Budget Percent By County
            FillBudgetPercentByCountyInExcel(worksheet, simulationYears);
        }

        private List<string> GetHeaders(List<int> simulationYears)
        {
            //static headers
            var headers = new List<string> {
                "District",
                "County"
            };

            //add years
            foreach (var year in simulationYears) { headers.Add(year.ToString()); }

            //return object
            return headers;
        }

        private void BuildUniqueDistrictCountyList(SimulationOutput reportOutputData)
        {
            if (reportOutputData?.InitialAssetSummaries?.Any() == true)
            {
                //create object
                IList<DistrictCounty> districtCountyList = [];

                foreach (var sectionSummary in reportOutputData.InitialAssetSummaries)
                {
                    //get and configure field values
                    var strDistrict = _summaryReportHelper.checkAndGetValue(sectionSummary.ValuePerTextAttribute, "DISTRICT");
                    var district = 0; if (!string.IsNullOrEmpty(strDistrict) && !string.IsNullOrWhiteSpace(strDistrict)) { district = Convert.ToInt16(strDistrict); }
                    var county = _summaryReportHelper.checkAndGetValue(sectionSummary.ValuePerTextAttribute, "COUNTY");

                    //check item in the list
                    var checkResultObject = districtCountyList
                                                .Where(w => w.District == district)
                                                .Where(w => w.County == county).FirstOrDefault();
                    if (checkResultObject == null)
                    {
                        //add object to list
                        districtCountyList.Add(new DistrictCounty() {
                            District = district,
                            County = county,
                        });
                    }
                }

                //sort district county list
                _uniqueDistrictCountyList = districtCountyList.OrderBy(o => o.District).ThenBy(t => t.County).ToList();
            }
        }

        private void BuildDistrictCountyCostList(SimulationOutput reportOutputData, List<int> simulationYears, Dictionary<string, List<TreatmentConsiderationDetail>> keyCashFlowFundingDetails, string primaryKey)
        {
            //build empty list
            _districtCountyCostList = new List<DistrictCountyCost>();
            if (_uniqueDistrictCountyList?.Any() == true && simulationYears?.Any() == true)
            {
                foreach (var districtCountyObject in _uniqueDistrictCountyList)
                {
                    //calculate cost
                    foreach (var year in simulationYears)
                    {
                        //cost
                        decimal sumOfCoveredCost = 0;
                        var simulationYearDetail = reportOutputData.Years.Where(w => w.Year == year).FirstOrDefault();
                        if (simulationYearDetail != null)
                        {
                            //asset detail list
                            var yearAssetIds = simulationYearDetail.Assets.Select(_ => _.AssetId).ToList();
                            var initialAssetSummaries = reportOutputData.InitialAssetSummaries.Where(_ => yearAssetIds.Contains(_.AssetId)).ToList();
                            var initialAssetDetailList = initialAssetSummaries
                                        .Where(s => _summaryReportHelper.checkAndGetValue(s.ValuePerTextAttribute, "DISTRICT")
                                            == districtCountyObject.District.ToString()
                                        && _summaryReportHelper.checkAndGetValue(s.ValuePerTextAttribute, "COUNTY")
                                            == districtCountyObject.County).ToList();
                            var initialAssetDetailListAssetIds = initialAssetDetailList.Select(_ => _.AssetId).ToList();
                            var assetDetailList = simulationYearDetail.Assets.Where(_ => initialAssetDetailListAssetIds.Contains(_.AssetId));
                            //get cost
                            if (assetDetailList?.Any() == true)
                            {
                                foreach (var section in assetDetailList)
                                {
                                    var primaryKeyValue = _summaryReportHelper.checkAndGetValue(section.ValuePerTextAttribute, primaryKey);
                                    var treatmentConsiderations = ((section.TreatmentCause == TreatmentCause.SelectedTreatment &&
                                                  section.TreatmentStatus == TreatmentStatus.Progressed) ||
                                                  (section.TreatmentCause == TreatmentCause.CashFlowProject &&
                                                  section.TreatmentStatus == TreatmentStatus.Progressed) ||
                                                  (section.TreatmentCause == TreatmentCause.CashFlowProject &&
                                                  section.TreatmentStatus == TreatmentStatus.Applied)) ?
                                                  keyCashFlowFundingDetails[primaryKeyValue] :
                                                  section.TreatmentConsiderations ?? [];

                                    sumOfCoveredCost += treatmentConsiderations.Sum(tc => tc.FundingCalculationOutput?.AllocationMatrix
                                        .Where(_ => _.Year == year).Sum(b => b.AllocatedAmount) ?? 0);                                    
                                }
                            }
                        }

                        //create object
                        var districtCountyCostObject = new DistrictCountyCost()
                        {
                            District = districtCountyObject.District,
                            County = districtCountyObject.County,
                            Year = year,
                            Cost = sumOfCoveredCost
                        };

                        //add to list
                        _districtCountyCostList.Add(districtCountyCostObject);
                    }
                }

                //calculate district sum
                if (_districtCountyCostList?.Any() == true)
                {
                    var districtSums = _districtCountyCostList
                        .GroupBy(d => new { d.District, d.Year })
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Cost));

                    foreach (var item in _districtCountyCostList)
                    {
                        item.DistrictSum = districtSums[new { item.District, item.Year }];
                    }
                }
            }
        }


        private void FillBudgetByCountyInExcel(ExcelWorksheet worksheet, List<int> simulationYears)
        {
            //Build Budget By County Headers
            var headers = GetHeaders(simulationYears);

            var rowNo = 1; var columnNo = 3;
            worksheet.Cells[rowNo, columnNo].Value = "Overall Dollars Recommended on Treatments by County";
            ExcelHelper.MergeCells(worksheet, rowNo, columnNo, rowNo, columnNo + (simulationYears.Count - 1));
            ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo + (simulationYears.Count - 1)]);

            //build header cells
            rowNo++; columnNo = 0;
            for (int column = 0; column < headers.Count; column++)
            {
                columnNo = column + 1;
                worksheet.Cells[rowNo, columnNo].Value = headers[column];
                ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, columnNo]);
            }

            //fill data in cells
            currentCell = new CurrentCell { Row = rowNo, Column = columnNo }; var currentRowIndex = 0;
            foreach (var districtCountyObject in _uniqueDistrictCountyList)
            {
                rowNo++; columnNo = 1; currentRowIndex++;
                worksheet.Cells[rowNo, columnNo++].Value = districtCountyObject.District;
                worksheet.Cells[rowNo, columnNo++].Value = districtCountyObject.County;

                //add cost values for each year
                if(_districtCountyCostList?.Any() == true && simulationYears?.Any() == true)
                {
                    foreach (var year in simulationYears)
                    {
                        var districtCountyCostObject = _districtCountyCostList
                                                            .Where(w => w.District == districtCountyObject.District)
                                                            .Where(w => w.County == districtCountyObject.County)
                                                            .Where(w => w.Year == year).FirstOrDefault();
                        if(districtCountyCostObject != null)
                        {
                            ExcelHelper.SetCurrencyFormat(worksheet.Cells[rowNo, columnNo]);
                            worksheet.Cells[rowNo, columnNo++].Value = districtCountyCostObject.Cost;
                        }
                    }

                    ExcelHelper.ApplyColor(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1], Color.LightGreen);
                    ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1]);
                }

                //check and add summary row
                var rowsPerDistrict = _uniqueDistrictCountyList.Where(w => w.District == districtCountyObject.District).Count();                
                if (currentRowIndex > rowsPerDistrict-1)
                {
                    //reset index
                    rowNo++; columnNo = 1; currentRowIndex = 0;

                    //Add Summary Row
                    ExcelHelper.MergeCells(worksheet, rowNo, columnNo, rowNo, columnNo + 1);
                    worksheet.Cells[rowNo, columnNo].Value = "District " + districtCountyObject.District.ToString() + " Total";

                    //add total
                    columnNo += 2;
                    foreach (var year in simulationYears)
                    {
                        //get district sum
                        decimal districtCostSum = 0;
                        var _filteredDistrictCountyCostList = _districtCountyCostList
                                                            .Where(w => w.District == districtCountyObject.District)
                                                            .Where(w => w.Year == year).ToList();
                        if(_filteredDistrictCountyCostList?.Any() == true)
                        {
                            //calculate district sum
                            districtCostSum = _filteredDistrictCountyCostList.Sum(s => s.Cost);
                            foreach (var filteredDistrictCountyCostObject in _filteredDistrictCountyCostList) {
                                filteredDistrictCountyCostObject.DistrictSum = districtCostSum;
                            }
                        }

                        //set district sum value
                        ExcelHelper.SetCurrencyFormat(worksheet.Cells[rowNo, columnNo]);
                        worksheet.Cells[rowNo, columnNo++].Value = districtCostSum;
                    }

                    ExcelHelper.ApplyColor(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1], Color.Green);
                    ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1]);
                }
            }

            //add state total
            rowNo++; columnNo = 1; 

            //Add Summary Row
            ExcelHelper.MergeCells(worksheet, rowNo, columnNo, rowNo, columnNo + 1);
            worksheet.Cells[rowNo, columnNo].Value = "State Total";

            columnNo += 2;
            foreach (var year in simulationYears)
            {
                //get district sum
                var stateCostSum = _districtCountyCostList.Where(w => w.Year == year).ToList().Sum(s => s.Cost);

                //set district sum value
                ExcelHelper.SetCurrencyFormat(worksheet.Cells[rowNo, columnNo]);
                worksheet.Cells[rowNo, columnNo++].Value = stateCostSum;                
            }
            ExcelHelper.ApplyColor(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1], Color.Green);
            ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1]);

            //set curretn cell value
            currentCell.Row = rowNo; currentCell.Column = columnNo;
        }

        private void FillBudgetPercentByCountyInExcel(ExcelWorksheet worksheet, List<int> simulationYears)
        {
            //Build Budget By County Headers
            var headers = GetHeaders(simulationYears);

            var rowNo = currentCell.Row + 1; var columnNo = 3;
            worksheet.Cells[rowNo, columnNo].Value = "% of Overall Dollars Recommended on Treatments by County";
            ExcelHelper.MergeCells(worksheet, rowNo, columnNo, rowNo, columnNo + (simulationYears.Count - 1));

            //build header cells
            rowNo++; columnNo = 0;
            for (int column = 0; column < headers.Count; column++)
            {
                columnNo = column + 1;
                worksheet.Cells[rowNo, columnNo].Value = headers[column];
                ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, columnNo]);
            }

            //fill data in cells
            var percentFormat = @"0.00%";
            currentCell = new CurrentCell { Row = rowNo, Column = columnNo }; var currentRowIndex = 0;
            foreach (var districtCountyObject in _uniqueDistrictCountyList)
            {
                rowNo++; columnNo = 1; currentRowIndex++;
                worksheet.Cells[rowNo, columnNo++].Value = districtCountyObject.District;
                worksheet.Cells[rowNo, columnNo++].Value = districtCountyObject.County;

                //add percent values for each year
                if (_districtCountyCostList?.Any() == true && simulationYears?.Any() == true)
                {
                    foreach (var year in simulationYears)
                    {
                        var districtCountyCostObject = _districtCountyCostList
                                                            .Where(w => w.District == districtCountyObject.District)
                                                            .Where(w => w.County == districtCountyObject.County)
                                                            .Where(w => w.Year == year).FirstOrDefault();
                        if (districtCountyCostObject != null)
                        {
                            decimal districtPercent = 0;
                            if (districtCountyCostObject.DistrictSum > 0) {
                                districtPercent = Convert.ToDecimal(districtCountyCostObject.Cost / districtCountyCostObject.DistrictSum);
                            }
                            districtCountyCostObject.Percent = districtPercent;

                            worksheet.Cells[rowNo, columnNo].Style.Numberformat.Format = percentFormat;
                            worksheet.Cells[rowNo, columnNo++].Value = districtPercent;
                        }
                    }

                    ExcelHelper.ApplyColor(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1], Color.LightBlue);
                    ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1]);
                }

                //check and add summary row
                var rowsPerDistrict = _uniqueDistrictCountyList.Where(w => w.District == districtCountyObject.District).Count();
                if (currentRowIndex > rowsPerDistrict - 1)
                {
                    //reset index
                    rowNo++; columnNo = 1; currentRowIndex = 0;

                    //Add Summary Row
                    ExcelHelper.MergeCells(worksheet, rowNo, columnNo, rowNo, columnNo + 1);
                    worksheet.Cells[rowNo, columnNo].Value = "District " + districtCountyObject.District.ToString() + " Total";

                    //add total percentage
                    columnNo += 2;
                    foreach (var year in simulationYears)
                    {
                        //get district sum
                        var districtPercentSum = _districtCountyCostList
                                                            .Where(w => w.District == districtCountyObject.District)
                                                            .Where(w => w.Year == year).ToList().Sum(s => s.Percent);

                        //set district sum value
                        worksheet.Cells[rowNo, columnNo].Style.Numberformat.Format = percentFormat;
                        worksheet.Cells[rowNo, columnNo++].Value = districtPercentSum;
                    }

                    ExcelHelper.ApplyColor(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1], Color.CadetBlue);
                    ExcelHelper.ApplyBorder(worksheet.Cells[rowNo, 1, rowNo, columnNo - 1]);
                }
            }

            //set current cell
            currentCell.Row = rowNo; currentCell.Column = columnNo;
        }

        #endregion Private methods
    }
}
