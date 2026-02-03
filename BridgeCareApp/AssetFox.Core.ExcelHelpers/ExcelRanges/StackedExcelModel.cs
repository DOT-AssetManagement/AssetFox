
﻿using System.Collections.Generic;
using AssetFox.Core.ExcelHelpers;

namespace AssetFox.Core.ExcelHelpers
{
    public class StackedExcelModel: IExcelModel
    {
        public List<IExcelModel> Content { get; set; }

        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }
}
