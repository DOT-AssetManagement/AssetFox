using System;

using AssetFox.Core.ExcelHelpers;
using OfficeOpenXml;

namespace AssetFox.Core.ExcelHelpers
{
    public class ExcelFormulaModel: IExcelModel
    {
        public Func<ExcelRange, string> Formula { get; set; }
        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }
}
