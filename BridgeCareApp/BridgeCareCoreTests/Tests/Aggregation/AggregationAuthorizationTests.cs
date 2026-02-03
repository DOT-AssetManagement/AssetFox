using System.Security.Claims;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Utils;
using AssetFoxCoreTests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class AggregationAuthorizationTests
    {
        // All authorization tests are effectively testing the contents of the file
        // rolesToClaimsMapping.json.

        private const string PolicyName = "TestAggregationPolicy";

        [Fact]
        public async Task UserIsAggregateNetworkDataAuthorized()
        {
            // Admin test authorize
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkAggregateAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsAggregateNetworkDataAuthorized_B2C()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkAggregateAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.B2C, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }
    }
}
