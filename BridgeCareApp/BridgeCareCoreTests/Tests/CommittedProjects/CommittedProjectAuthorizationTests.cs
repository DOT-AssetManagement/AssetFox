using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCore.Utils;
using BridgeCareCoreTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static BridgeCareCore.Security.SecurityConstants;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BridgeCareCoreTests.Tests.CommittedProjects
{
    public class CommittedProjectAuthorizationTests
    {
        [Fact]
        public async Task UserIsEditor_ModifyCommittedProjects_Unauthorized()
        {
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policy.ModifyCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectModifyAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Editor });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, Policy.ModifyCommittedProjects);
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
                    options.AddPolicy(Policy.ModifyCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectModifyAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, Policy.ModifyCommittedProjects);
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
                    options.AddPolicy(Policy.ViewCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewPermittedAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewAnyAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Editor });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, Policy.ViewCommittedProjects);
            // Assert
            Assert.True(allowed.Succeeded);
        }

        [Fact]
        public async Task ImportCommittedProjects_UserIsAdmin_Allowed()
        {
            var policy = Policy.ImportCommittedProjects;
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(policy,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectImportAnyAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectImportPermittedAccess
                        ));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.Esec, new List<string> { Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, policy);
            // Assert
            Assert.True(allowed.Succeeded);

        }
    }
}
