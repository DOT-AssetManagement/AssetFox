
﻿using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.ExcelHelpers
{
    public class AutoFitColumnsExcelWorksheetContentModel: IExcelWorksheetContentModel
    {
        public double MinWidth { get; set; }


        public TOutput Accept<TOutput, THelper>(IExcelWorksheetModelVisitor<THelper, TOutput> visitor, THelper helper)
            => visitor.Visit(this, helper);
    }
}
