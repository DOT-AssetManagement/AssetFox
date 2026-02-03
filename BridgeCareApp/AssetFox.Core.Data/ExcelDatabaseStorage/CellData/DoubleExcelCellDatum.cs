using System.Text.Json;

namespace AssetFox.Core.Data.ExcelDatabaseStorage
{
    public class DoubleExcelCellDatum: IExcelCellDatum
    {
        public double Value { get; set; }
        public T Accept<T, THelper>(IExcelCellDatumVisitor<THelper, T> visitor, THelper helper)
        {
            var returnValue = visitor.Visit(this, helper);
            return returnValue;
        }
    }
}
