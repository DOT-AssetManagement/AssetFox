using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class TreatmentLibraryUserEntity: LibraryUserBaseEntity
    {
        public virtual TreatmentLibraryEntity TreatmentLibrary { get; set; }
    }
}
