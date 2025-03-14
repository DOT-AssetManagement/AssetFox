using System;
using System.Diagnostics;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DataUnitTests;
using Microsoft.EntityFrameworkCore;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
{
    public static class TestHelper
    {
        static TestHelper()
        {
            try
            {
                var config = TestConfiguration.Get();
                var connectionString = TestConnectionStrings.BridgeCare(config);
                var options = new DbContextOptionsBuilder<IAMContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var dbContext = new IAMContext(options);
                UnitOfWork = new UnitOfDataPersistenceWork(config, dbContext);
                DatabaseResetter.ResetDatabase(UnitOfWork);
            }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                throw;
            }
        }
        public static IAMContext DbContext => UnitOfWork.Context;

        public static readonly UnitOfDataPersistenceWork UnitOfWork;
    }
}
