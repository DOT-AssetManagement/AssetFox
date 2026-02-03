using System.Collections.Generic;
using AssetFox.Core.Data.Attributes;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IAttributeMetaDataRepository
    {
        List<Attribute> GetAllAttributes(System.Guid dataSourceId);
    }
}
