using AssetFox.Core.DataMiner.Attributes;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class DirectionalRouteEntity : RouteEntity
    {
        public Direction Direction { get; set; }
    }
}
