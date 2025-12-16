using System.Collections.Generic;
using AssetFox.Core.ExcelHelpers;
using TModel = AssetFox.Core.ExcelHelpers.Tables.ExcelHeaderWithContentModel<AssetFox.Core.DTOs.TreatmentCostDTO>;

namespace AssetFoxCore.Services.Treatment
{

    public static class TreatmentCostHeaderWithContentModels
    {
        public static TModel EquationExpression
            => new()
            {
                Header = StackedExcelModels.BoldText(TreatmentExportStringConstants.Equation),
                Content = cost => ExcelValueModels.String(cost.Equation.Expression),
            };

        public static TModel CriterionExpression
            => new()
            {
                Header = StackedExcelModels.BoldText(TreatmentExportStringConstants.CostCriterion),
                Content = cost => ExcelValueModels.String(cost.CriterionLibrary.MergedCriteriaExpression),
            };

        public static IEnumerable<TModel> TreatmentCostExport()
        {
            yield return EquationExpression;
            yield return CriterionExpression;
        }
    }
}
