using System;
using System.Diagnostics;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataUnitTests;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UnitTestsCore.TestUtils
{
    public static class TestHelper
    {
        // This static constructor runs once per test class
        // that uses TestHelper. As of 4/7/2025, that means
        // it runs twice per test run. It takes about 15 seconds.
        // It might be preferable to run it only once per test run,
        // but it's not obvious how to accomplish that.
        static TestHelper()
        {
            try
            {
                var config = TestConfiguration.Get();
                var connectionString = TestConnectionStrings.BridgeCare(config);
                var options = new DbContextOptionsBuilder<IAMContext>()
                    .UseSqlServer(connectionString,
                    opts => opts.CommandTimeout(3600))
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
