using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Utils;
using AssetFoxCore.Utils.Interfaces;
using AssetFoxCoreTests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Policy = AssetFoxCore.Security.SecurityConstants.Policy;

namespace AssetFoxCoreTests.Tests
{
    public class CalculatedAttributeAuthorizationTests
    {
        private const string PolicyName = "TestCalculatedAttributePolicy";

        [Fact]
        public async Task UserIsModifyCalculatedAttributesFromScenarioAuthorized()
        {
            // Non-admin unauthorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesChangeInScenarioAccess,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesModifyFromScenarioAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.ReadOnly });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.False(allowed.Succeeded);
        }

        [Fact]
        public async Task UserIsModifyCalculatedAttributesFromLibraryAuthorized()
        {
            // Admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesModifyFromLibraryAccess,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesChangeDefaultLibraryAccess));
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
        public async Task UserIsModifyCalculatedAttributesFromScenarioAuthorized_B2C()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesChangeInScenarioAccess,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CalculatedAttributesModifyFromScenarioAccess));
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
