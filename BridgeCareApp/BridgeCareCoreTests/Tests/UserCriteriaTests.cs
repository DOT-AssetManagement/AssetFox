using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
    public class UserCriteriaTests
    {
        [Fact]
        public async Task UserIsViewUserCriteriaAuthorized()
        {
            // Admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("ViewUserCriteriaClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.UserCriteriaViewAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "ViewUserCriteriaClaim");
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsModifyUserCriteriaAuthorized()
        {
            // Non-admin unauthorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("ModifyUserCriteriaClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.UserCriteriaModifyAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.ReadOnly });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "ModifyUserCriteriaClaim");
            // Assert
            Assert.False(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsViewUserCriteriaAuthorized_B2C()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("ViewUserCriteriaClaim",
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.UserCriteriaViewAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(AssetFoxCore.Security.SecurityConstants.SecurityTypes.B2C, new List<string> { AssetFoxCore.Security.SecurityConstants.Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, "ViewUserCriteriaClaim");
            // Assert
            Assert.True(allowed.Succeeded);
        }
    }
}
