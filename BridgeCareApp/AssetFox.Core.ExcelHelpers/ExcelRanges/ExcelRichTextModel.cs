using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.ExcelHelpers
{
    public class ExcelRichTextModel: IExcelModel
    {
        public string Text { get; set; }
        public bool Bold { get; set; }
        public float? FontSize { get; set; }
        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }
}
