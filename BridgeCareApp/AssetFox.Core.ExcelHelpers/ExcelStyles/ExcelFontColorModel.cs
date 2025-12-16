using System.Drawing;

namespace AppliedResearchAssociates.iAM.ExcelHelpers
{
    public class ExcelFontColorModel : IExcelModel
    {
        public Color Color { get; set; }

        public FontStyle FontStyle { get; set; }

        public T Accept<THelper, T>(IExcelModelVisitor<THelper, T> visitor, THelper helper) =>
            visitor.Visit(this, helper);
    }    
}
