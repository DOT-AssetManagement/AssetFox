using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AnnouncementEntity : BaseEntity
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}
