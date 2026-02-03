using System;

namespace AssetFox.Core.ExcelHelpers.Tables
{
    public class ExcelHeaderWithContentModel<TData>
    {
        public IExcelModel Header { get; set; }
        public Func<TData, IExcelModel> Content { get; set; }   
    }
}
