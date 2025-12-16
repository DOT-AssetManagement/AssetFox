using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFox.Core.Data.Helpers;

namespace AssetFox.Core.Data.ExcelDatabaseStorage
{
    public static class IExcelCellDatumExtensions
    {
        public static object ObjectValue(this IExcelCellDatum datum)
        {
            var visitor = ExcelCellDatumValueGetter.Instance;
            var returnValue = datum.Accept(visitor, Unit.Default);
            return returnValue;
        }
    }
}
