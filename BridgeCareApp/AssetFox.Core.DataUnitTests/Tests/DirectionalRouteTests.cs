using Xunit;
using Moq;
using AssetFox.Core.Data;

namespace AssetFox.Core.DataUnitTests.Tests
{
    public class DirectionalRouteTests
    {
        private readonly DirectionalRoute directionalRoute;
        private const string locationIdentifier = "TestUniqueId";
        private const string locationIdentifier2 = "TestUniqueId2";

        public DirectionalRouteTests()
        {
            directionalRoute = new DirectionalRoute(locationIdentifier, Direction.N);
        }        

        [Fact]
        public void MatchOnTest()
        {
            // Act
            var mockDirectionalRoute = Init(locationIdentifier, Direction.S);

            var doesMatch = directionalRoute.MatchOn(mockDirectionalRoute.Object);

            // Assert
            Assert.True(doesMatch);
        }

        [Fact]
        public void MatchOnOnlyLocationIdentifierTest()
        {
            // Arrange
            var mockDirectionalRoute = Init(locationIdentifier, Direction.S);

            // Act
            var doesMatch = directionalRoute.MatchOn(mockDirectionalRoute.Object);

            // Assert
            Assert.True(doesMatch);
        }

        [Fact]
        public void MatchOnFalseTest()
        {
            // Arrange
            var mockDirectionalRoute = Init(locationIdentifier2, Direction.N);

            // Act
            var doesMatch = directionalRoute.MatchOn(mockDirectionalRoute.Object);

            // Assert
            Assert.False(doesMatch);
        }

        private Mock<DirectionalRoute> Init(string locationIdentifier, Direction direction)
        {
            var mockDirectionalRoute = new Mock<DirectionalRoute>(locationIdentifier, direction);
            return mockDirectionalRoute;
        }
    }
}
