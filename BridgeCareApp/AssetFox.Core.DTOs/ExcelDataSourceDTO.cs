using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class ExcelDataSourceDTO : BaseDataSourceDTO
    {
        public ExcelDataSourceDTO() : base(DataSourceTypeStrings.Excel.ToString())
        {
            Secure = false;
        }

        public string LocationColumn { get; set; }
        public string DateColumn { get; set; }
        public override bool Validate() => true;
    }
}
