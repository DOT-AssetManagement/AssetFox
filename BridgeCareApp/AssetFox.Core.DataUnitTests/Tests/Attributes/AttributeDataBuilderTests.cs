using Xunit;
using Moq;
using System;
using AssetFox.Core.Data.Attributes;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;
using System.Collections.Generic;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.DTOs;
using AssetFox.Core.Common;

namespace AssetFox.Core.DataUnitTests.Tests.Attributes
{
    public class AttributeDataBuilderTests
    {
        [Fact]
        public void GetDataForStringTypeTest()
        {
            // Arrange
            var mockAttributeConnection = Init(AttributeTypeNames.String);

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
            var mockAttributeConnection = Init(AttributeTypeNames.Number);

            var result = AttributeDataBuilder.GetData(mockAttributeConnection.Object);
            Assert.NotNull(result);
            Assert.IsType<List<IAttributeDatum>>(result);
        }

        private Mock<AttributeConnection> Init(string type)
        {
            var mockAttribute = new Mock<Attribute>(Guid.Empty, CommonTestParameterValues.Name, type, CommonTestParameterValues.RuleType, CommonTestParameterValues.TestCommand, Data.ConnectionType.MSSQL, CommonTestParameterValues.ConnectionString, Guid.Empty, false, false);
            var mockDataSource = new Mock<SQLDataSourceDTO>();
            var mockAttributeConnection = new Mock<AttributeConnection>(mockAttribute.Object, mockDataSource.Object);
            mockAttributeConnection.Setup(m => m.GetData<It.IsAnyType>()).Returns(new List<IAttributeDatum>());
            return mockAttributeConnection;

        }
    }
}
