using Xunit;
using AssetFox.Core.Data.Attributes;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.Repositories;

namespace AssetFox.Core.DataUnitTests.Tests.Attributes
{
    public class SqlAttributeConnectionTests// also create tests for ExcelAttributeConnection
    {
        [Fact(Skip ="Sql attribute connections are not being used. This sporadically fails  with transient error for reasons WJ is unclear about.")]
        public void GetData_StringAttributeInDatabase_Gets()
        {
            // Arrange
            var config = TestConfiguration.Get();
            var connectionString = TestConnectionStrings.BridgeCare(config);
            var unitOfWork = UnitOfWorkSetup.New(config);
            var dataSource = DataSourceTestSetup.DtoForSqlDataSourceInDb(unitOfWork, connectionString);
            var attribute = AttributeConnectionAttributes.String(connectionString, dataSource.Id);
            unitOfWork.AttributeRepo.UpsertAttributes(attribute);
            var sqlAttributeConnection = new SqlAttributeConnection(attribute, dataSource);

            // Act
            var result = sqlAttributeConnection.GetData<string>();

            // Assert
            var resultElement = result.Single();
            Assert.IsType<AttributeDatum<string>>(resultElement);
        }
    }
}
