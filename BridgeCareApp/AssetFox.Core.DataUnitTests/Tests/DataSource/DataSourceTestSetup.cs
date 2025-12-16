using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataUnitTests.Tests
{
    public static class DataSourceTestSetup
    {
        public static SQLDataSourceDTO DtoForSqlDataSourceInDb(IUnitOfWork unitOfWork, string connectionString)
        {
            var dataSource = SqlDataSourceDtos.WithConnectionString(connectionString);
            unitOfWork.DataSourceRepo.UpsertDatasource(dataSource);
            return dataSource;
        }

        public static ExcelDataSourceDTO DtoForExcelDataSourceInDb(IUnitOfWork unitOfWork)
        {
            var dataSource = ExcelDataSourceDtos.WithColumnNames("Inspection_Date", "BRKEY");
            unitOfWork.DataSourceRepo.UpsertDatasource(dataSource);
            return dataSource;
        }       
    }
}
