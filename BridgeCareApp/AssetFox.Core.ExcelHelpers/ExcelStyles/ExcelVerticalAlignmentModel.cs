using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AssetFox.Core.ExcelHelpers;
using OfficeOpenXml.Style;

namespace AssetFox.Core.ExcelHelpers
{
    public class ExcelVerticalAlignmentModel : IExcelModel
    {
        public ExcelVerticalAlignment Alignment { get; set; }

        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }
}
