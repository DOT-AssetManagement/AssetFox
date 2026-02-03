using System;
using AssetFox.Core.DataMiner;

namespace AssetFox.Core.DataPersistenceCore.Models
{
    public class Attribute
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Command { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public string ConnectionString { get; set; }
    }
}
