using Xunit;
using System;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.DataUnitTests.TestUtils;
using AssetFox.Core.Common;

namespace AssetFox.Core.DataUnitTests.Tests.Attributes
{
    public class AttributeFactoryTests
    {
        private const string DEFAULT_RULE = "Last";

        [Fact]
        public void CreateForStringTypeTest()
        {
            // Arrange
            var attributeMetaDatum = Init(AttributeTypeNames.String, string.Empty);

            // Act
            var result = AttributeFactory.Create(attributeMetaDatum, Guid.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TextAttribute>(result);
        }

        [Fact]
        public void CreateForStringTypeWithValueTest()
        {
            // Arrange
            var attributeMetaDatum = Init(AttributeTypeNames.String, CommonTestParameterValues.StringValue);

            // Act
            var result = AttributeFactory.Create(attributeMetaDatum, Guid.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TextAttribute>(result);
        }

        [Fact]
        public void CreateForNumberDoubleTypeTest()
        {
            // Arrange
            var attributeMetaDatum = Init(AttributeTypeNames.Number, CommonTestParameterValues.DoubleNumber);

            // Act
            var result = AttributeFactory.Create(attributeMetaDatum, Guid.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NumericAttribute>(result);
        }

        [Fact]
        public void CreateForNumberIntTypeTest()
        {
            // Arrange
            var attributeMetaDatum = Init(AttributeTypeNames.Number, CommonTestParameterValues.IntNumber);

            // Act
            var result = AttributeFactory.Create(attributeMetaDatum, Guid.Empty );

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NumericAttribute>(result);
        }


        [Fact]
        public void CreateForNumberInvalidValueTest()
        {
            // Arrange
            var attributeMetaDatum = Init(AttributeTypeNames.Number, string.Empty);

            // Act, Assert
            Assert.Throws<InvalidCastException>(() => AttributeFactory.Create(attributeMetaDatum, Guid.Empty));
        }

        [Fact]
        public void CreateForNoTypeTest()
        {
            // Arrange
            var attributeMetaDatum = Init(string.Empty, string.Empty);

            // Act, Assert
            Assert.Throws<InvalidOperationException>(() => AttributeFactory.Create(attributeMetaDatum, Guid.Empty));
        }

        private AttributeMetaDatum Init(string type, string defaultValue)
        {
            var attributeMetaDatum = new AttributeMetaDatum();
            attributeMetaDatum.Type = type;
            attributeMetaDatum.AggregationRule = DEFAULT_RULE;
            attributeMetaDatum.DefaultValue = defaultValue;
            return attributeMetaDatum;
        }
    }
}
