using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class SQLDataSourceDTO : BaseDataSourceDTO
    {
        public SQLDataSourceDTO() : base(DataSourceTypeStrings.SQL.ToString())
        {
            Secure = true;
        }

        public string ConnectionString { get; set; }

        public override bool Validate() => true;
    }
}
