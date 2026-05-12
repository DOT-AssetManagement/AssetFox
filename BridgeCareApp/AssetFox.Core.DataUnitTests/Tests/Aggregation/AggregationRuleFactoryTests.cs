using Xunit;
using Moq;
using System;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;
using AssetFox.Core.Data.Aggregation;
using System.Reflection;

namespace AssetFox.Core.DataUnitTests.Tests.Aggregation
{
    public class AggregationRuleFactoryTests
    {
        private readonly Guid guId = Guid.Empty;
        private const string InvalidAggregationRuleType = "OTHER";

        [Fact(Skip = "Resurrect if we bring back aggregation.")]
        public void CreateAverageRuleTest()
        {
            // Arrange
            var mockAttribute = Init("Number","Average");

            // Act
            var result = AggregationRuleFactory.CreateNumericRule(mockAttribute.Object);

            // Assert
            Assert.IsType<AverageAggregationRule>(result);
        }

        [Fact(Skip = "Resurrect if we bring back aggregation.")]
        public void CreateNumericLastRuleTest()
        {
            // Arrange
            var mockAttribute = Init("Number", "Last");

            // Act
            var result = AggregationRuleFactory.CreateNumericRule(mockAttribute.Object);

            // Assert
            Assert.IsType<LastNumericAggregationRule>(result);
        }

        [Fact]
        public void CreatePredominantNumericRuleTest()
        {
            // Arrange
            var mockAttribute = Init("Number", "Predominant");

            // Act
            var result = AggregationRuleFactory.CreateNumericRule(mockAttribute.Object);

            // Assert
            Assert.IsType<PredominantNumericAggregationRule>(result);
        }

        [Fact(Skip = "Resurrect if we bring back aggregation.")]
        public void CreateNumericRuleExceptionTest()
        {
            // Arrange
            var mockAttribute = Init("Number", InvalidAggregationRuleType);

            // Act, Assert
            var exception = Assert.Throws<TargetInvocationException>(() => AggregationRuleFactory.CreateNumericRule(mockAttribute.Object));
            var innerException = exception.InnerException;
            Assert.True(innerException is InvalidOperationException);
        }

        [Fact]
        public void CreatePredominantTextRuleTest()
        {
            // Arrange
            var mockAttribute = Init("String", "Predominant");

            // Act
            var result = AggregationRuleFactory.CreateTextRule(mockAttribute.Object);

            // Assert
            Assert.IsType<PredominantTextAggregationRule>(result);
        }

        [Fact(Skip = "Resurrect if we bring back aggregation.")]
        public void CreateTextLastRuleTest()
        {
            // Arrange
            var mockAttribute = Init("String", "Last");

            // Act
            var result = AggregationRuleFactory.CreateTextRule(mockAttribute.Object);

            // Assert
            Assert.IsType<LastTextAggregationRule>(result);
        }

        [Fact(Skip = "Resurrect if we bring back aggregation.")]
        public void CreateTextRuleExceptionTest()
        {
            // Arrange
            var mockAttribute = Init("String", InvalidAggregationRuleType);

            // Act, Assert
            var exception = Assert.Throws<TargetInvocationException>(() => AggregationRuleFactory.CreateTextRule(mockAttribute.Object));
            var innerException = exception.InnerException;
            Assert.True(innerException is InvalidOperationException);
        }

        public Mock<Attribute> Init(string dataType, string aggregationRuleType)
        {
            return new Mock<Attribute>(guId, null, dataType, aggregationRuleType, null, Data.ConnectionType.MSSQL, null, Guid.Empty, false, false);
        }
    }
}
