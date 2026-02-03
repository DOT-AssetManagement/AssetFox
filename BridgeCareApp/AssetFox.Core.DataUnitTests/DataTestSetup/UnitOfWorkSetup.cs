using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssetFox.Core.DataUnitTests
{
    public static class UnitOfWorkSetup
    {
        public static UnitOfDataPersistenceWork New(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BridgeCareConnex");
            var options = new DbContextOptionsBuilder<IAMContext>()
                .UseSqlServer(connectionString,
                opts => opts.CommandTimeout(3600))
                .Options;
            var dbContext = new IAMContext(options);
            var unitOfWork = new UnitOfDataPersistenceWork(configuration, dbContext);
            return unitOfWork;
        }
    }
}
