using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Utils;
using AssetFoxCoreTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static AssetFoxCore.Security.SecurityConstants;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AssetFoxCoreTests.Tests.CommittedProjects
{
    public class CommittedProjectAuthorizationTests
    {
        private const string PolicyName = "TestCommittedProjectsPolicy";

        [Fact]
        public async Task UserIsEditor_ModifyCommittedProjects_Unauthorized()
        {
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectModifyAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Editor });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.False(allowed.Succeeded);
        }

        [Fact]
        public async Task UserIsAdmin_ModifyCommittedProjects_Unauthorized()
        {
             var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectModifyAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }

        [Fact]
        public async Task UserIsEditor_CanViewCommittedProjects()
        {
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectViewPermittedAccess,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectViewAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Editor });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }

        [Fact]
        public async Task ImportCommittedProjects_UserIsAdmin_Allowed()
        {
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectImportAnyAccess,
                                                      AssetFoxCore.Security.SecurityConstants.Claim.CommittedProjectImportPermittedAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);

        }
    }
}
