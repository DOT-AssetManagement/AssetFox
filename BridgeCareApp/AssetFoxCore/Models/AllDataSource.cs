using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCore.Models
{
    public class AllDataSource : BaseDataSourceDTO
    {
        public AllDataSource() : base(DataSourceTypeStrings.All.ToString())
        {
            Secure = false;
        }

        public override string Type { get; set; }
        public string ConnectionString { get; set; }
        public string LocationColumn { get; set; }
        public string DateColumn { get; set; }

        public override bool Validate() => true;
    }
}
