using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.DataUnitTests
{
    public static class DatabaseResetter
    {
        public static void ResetDatabase(UnitOfDataPersistenceWork unitOfWork)
        {
            unitOfWork.Context.Database.EnsureDeleted();
            unitOfWork.Context.Database.Migrate();
        }
    }
}
