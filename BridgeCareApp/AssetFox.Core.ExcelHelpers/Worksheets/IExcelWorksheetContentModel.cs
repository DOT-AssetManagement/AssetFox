
﻿using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.ExcelHelpers
{
    public interface IExcelWorksheetContentModel
    {
        public TOutput Accept<TOutput, THelper>(IExcelWorksheetModelVisitor<THelper, TOutput> visitor, THelper helper);
    }
}
