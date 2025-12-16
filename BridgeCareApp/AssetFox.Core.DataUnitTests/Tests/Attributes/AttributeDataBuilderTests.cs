using Xunit;
using Moq;
using System;
using AssetFox.Core.Data.Attributes;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;
using System.Collections.Generic;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataUnitTests.Tests.Attributes
{
    public class AttributeDataBuilderTests
    {
        private Mock<Attribute> mockAttribute;
        private Mock<AttributeConnection> mockAttributeConnection;

        [Fact]
        public void GetDataForStringTypeTest()
        {
            // Arrange
            Init(AttributeTypeNames.String);

            // Act
            var result = AttributeDataBuilder.GetData(mockAttributeConnection.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<IAttributeDatum>>(result);
        }

        [Fact]
        public void GetDataForNumberTypeTest()
        {
            // Arrange
            Init(AttributeTypeNames.Number);

            var result = AttributeDataBuilder.GetData(mockAttributeConnection.Object);
            Assert.NotNull(result);
            Assert.IsType<List<IAttributeDatum>>(result);
        }

        private void Init(string type)
        {
            mockAttribute = new Mock<Attribute>(Guid.Empty, CommonTestParameterValues.Name, type, CommonTestParameterValues.RuleType, CommonTestParameterValues.TestCommand, Data.ConnectionType.MSSQL, CommonTestParameterValues.ConnectionString, Guid.Empty, false, false);
            var mockDataSource = new Mock<SQLDataSourceDTO>();
            mockAttributeConnection = new Mock<AttributeConnection>(mockAttribute.Object, mockDataSource.Object);
            mockAttributeConnection.Setup(m => m.GetData<It.IsAnyType>()).Returns(new List<IAttributeDatum>());
        }
    }
}
