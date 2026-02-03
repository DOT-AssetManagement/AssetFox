using System.Security.Claims;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Utils;
using AssetFoxCoreTests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class NetworkTests
    {
        [Fact]
        public async Task UserIsNetworkViewAuthorized()
        {
            // Non-admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("NetworkViewClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkViewAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Editor });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "NetworkViewClaim");
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsNetworkAddAuthorized()
        {
            // Admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("NetworkAddClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkAddAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "NetworkAddClaim");
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsAggregateNetworkAuthorized()
        {
            // Non-admin unauthorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("NetworkAggregateClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkAggregateAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.ReadOnly });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "NetworkAggregateClaim");
            // Assert
            Assert.False(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsNetworkViewAuthorized_B2C()
        {
            // Non-admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("NetworkViewClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.NetworkViewAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.B2C, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "NetworkViewClaim");
            // Assert
            Assert.True(allowed.Succeeded);
        }
    }
}
