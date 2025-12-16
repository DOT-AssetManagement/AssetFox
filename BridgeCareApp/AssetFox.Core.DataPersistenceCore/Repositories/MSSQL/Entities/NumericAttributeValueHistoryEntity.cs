using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class NumericAttributeValueHistoryEntity : AttributeValueHistoryEntity
    {
        public int Year { get; set; }

        public double Value { get; set; }
    }
}
