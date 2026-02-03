using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.ExcelHelpers
{
    public class ExcelIntegerValueModel: IExcelModel
    {
        public int Value { get; set; }

        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }
}
