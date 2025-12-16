using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class AttributeValueHistoryEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid SectionId { get; set; }

        public Guid AttributeId { get; set; }

        public virtual AnalysisMaintainableAssetEntity AnalysisMaintainableAsset { get; set; }

        public virtual AttributeEntity Attribute { get; set; }
    }
}
