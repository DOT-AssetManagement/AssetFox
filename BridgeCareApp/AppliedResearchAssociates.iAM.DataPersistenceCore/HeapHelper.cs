using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Common;
using Microsoft.EntityFrameworkCore;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore
{
    internal static class HeapHelpers
    {
        public static void EnsureFresh(this IAMContext db, string live)
        {
            var heap = live.Stage();
            db.Database.ExecuteSqlRaw($@"
                IF OBJECT_ID('{heap}', 'U') IS NULL
                SELECT TOP(0) * INTO {heap} FROM {live};
                TRUNCATE TABLE {heap};");                  // clears orphans from staging tables
        }
    }
}
