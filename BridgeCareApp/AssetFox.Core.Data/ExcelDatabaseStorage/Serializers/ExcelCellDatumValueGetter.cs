using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.Data.ExcelDatabaseStorage.CellData;
using AssetFox.Core.Data.Helpers;

namespace AssetFox.Core.Data.ExcelDatabaseStorage.Serializers
{
    public class ExcelCellDatumValueGetter : IExcelCellDatumVisitor<Unit, object>
    {
        public static ExcelCellDatumValueGetter Instance => new ExcelCellDatumValueGetter();
        public object Visit(StringExcelCellDatum datum, Unit helper)
        {
            return datum.Value;
        }

        public object Visit(DoubleExcelCellDatum datum, Unit helper)
        {
            return datum.Value;
        }

        public object Visit(DateTimeExcelCellDatum datum, Unit helper)
        {
            return datum.Value;
        }

        public object Visit(EmptyExcelCellDatum datum, Unit helper)
        {
            return null;
        }
    }
}
