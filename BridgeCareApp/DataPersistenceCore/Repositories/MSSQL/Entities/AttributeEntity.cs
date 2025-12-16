using System;
using System.ComponentModel.DataAnnotations;
using AssetFox.Core.DataMiner;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class AttributeEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }
        public ConnectionType ConnectionType { get; set; }
    }
}
