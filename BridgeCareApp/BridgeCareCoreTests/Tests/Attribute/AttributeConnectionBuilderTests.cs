using Xunit;
using System;
using AssetFox.Core.Data.Attributes;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;
using Moq;
using AssetFox.Core.Data;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.DTOs;
using AssetFoxCore.Services;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.iAM;

namespace AssetFoxCoreTests.Tests
{
    public class AttributeConnectionBuilderTests
    {
        private Mock<Attribute> mockAttribute;
        private Mock<SQLDataSourceDTO> mockSQLDataSourceDTO = new Mock<SQLDataSourceDTO>();
        private Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();

        [Fact]
        public void BuildWithMsSqlTest()
        {
            // Arrange
            Init(ConnectionType.MSSQL);

            // Act
            var result = AttributeConnectionBuilder.Build(mockAttribute.Object, mockSQLDataSourceDTO.Object, mockUnitOfWork.Object);           

            // Assert
            Assert.IsType<SqlAttributeConnection>(result);
        }

        private void Init(ConnectionType connectionType)
        {
            mockAttribute = new Mock<Attribute>(Guid.Empty, CommonTestParameterValues.Name, AttributeTypeNames.String, CommonTestParameterValues.RuleType, CommonTestParameterValues.TestCommand, connectionType, CommonTestParameterValues.ConnectionString, Guid.Empty, false, false);
        }
    }
}
