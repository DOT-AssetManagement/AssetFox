using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BaseEntityPropertySetter
    {
        public static void SetBaseEntityProperties(BaseEntity baseEntity, BaseEntityProperties baseEntityProperties)
        {
            if (baseEntity != null && baseEntityProperties != null)
            {
                baseEntity.CreatedBy = baseEntityProperties.CreatedBy;
                baseEntity.LastModifiedBy = baseEntityProperties.LastModifiedBy;
                baseEntity.CreatedDate = DateTime.Now;
                baseEntity.LastModifiedDate = DateTime.Now;
            }
        }
    }
}
