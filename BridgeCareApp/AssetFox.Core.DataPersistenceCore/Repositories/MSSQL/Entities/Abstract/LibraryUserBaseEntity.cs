using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract
{
    public abstract class LibraryUserBaseEntity : BaseEntity
    {
        public Guid LibraryId { get; set; }
        public Guid UserId { get; set; }
        public int AccessLevel { get; set; }
        public virtual UserEntity User { get; set; }
    }
}
