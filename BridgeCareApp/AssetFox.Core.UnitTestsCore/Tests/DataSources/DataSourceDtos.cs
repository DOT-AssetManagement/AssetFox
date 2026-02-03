using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using Microsoft.Extensions.Configuration;

namespace AssetFox.Core.UnitTestsCore.Tests.DataSources
{
    public static class DataSourceDtos
    {
        public static SQLDataSourceDTO TestConfigurationSql(Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var config = TestConfiguration.Get();
            var name = RandomStrings.WithPrefix("dataSource");
            var connectionString = config.GetConnectionString("BridgeCareConnex");
            var dto = new SQLDataSourceDTO
            {
                ConnectionString = connectionString,
                Id = resolveId,
                Name = name,
            };
            return dto;
        }

        public static ExcelDataSourceDTO TestConfigurationExcel(string locationColumn, Guid? id = null)
        {

            var resolveId = id ?? Guid.NewGuid();
            var name = RandomStrings.WithPrefix("dataSource");
            var dataSource = new ExcelDataSourceDTO
            {
                LocationColumn = locationColumn,
                Id = resolveId,
                Name = name,
            };
            return dataSource;
        }
    }
}
