using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataUnitTests.Tests;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.DataSources;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.ExcelRawData
{
    public class ExcelRawDataRepositoryTests
    {
        [Fact]
        public void AddExcelRawData_DataSourceDoesNotExist_Throws()
        {
            var datasourceId = Guid.NewGuid();
            var dto = ExcelRawDataDtos.Dto(datasourceId);
            Assert.Throws<InvalidOperationException>(() => TestHelper.UnitOfWork.ExcelWorksheetRepository.AddExcelRawData(dto));
        }

        [Fact]
        public void AddExcelRawData_DataSourceExists_Does()
        {
            var datasource = DataSourceTestSetup.DtoForExcelDataSourceInDb(TestHelper.UnitOfWork);
            var datasourceId = datasource.Id;
            var dto = ExcelRawDataDtos.Dto(datasourceId);
            TestHelper.UnitOfWork.ExcelWorksheetRepository.AddExcelRawData(dto);
            var dataSourceInDb = TestHelper.UnitOfWork.ExcelWorksheetRepository.GetExcelRawDataByDataSourceId(datasourceId);
            ObjectAssertions.Equivalent(dto, dataSourceInDb);
        }
    }
}
