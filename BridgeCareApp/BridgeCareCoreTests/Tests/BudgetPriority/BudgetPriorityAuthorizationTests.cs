using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCore.Utils;
using static BridgeCareCore.Security.SecurityConstants;
using Xunit;
using BridgeCareCoreTests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BridgeCareCoreTests.Tests
{
    public class BudgetPriorityAuthorizationTests
    {
        private const string PolicyName = "TestBudgetPriorityPolicy";

        [Fact]
        public async Task UserIsViewBudgetPriorityFromLibraryAuthorized()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name, BridgeCareCore.Security.SecurityConstants.Claim.BudgetPriorityViewAnyFromLibraryAccess));
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
        public async Task UserIsModifyBudgetPriorityFromScenarioAuthorized()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.BudgetPriorityModifyAnyFromScenarioAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.BudgetPriorityModifyPermittedFromScenarioAccess));
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
        public async Task UserIsDeleteBudgetPriorityFromLibraryAuthorized()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.AnnouncementModifyAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.AttributesUpdateAccess));
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
        public async Task UserIsViewBudgetPriorityFromLibraryAuthorized_B2C()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name, BridgeCareCore.Security.SecurityConstants.Claim.BudgetPriorityViewAnyFromLibraryAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var claims = roleClaimsMapper.GetClaims(SecurityTypes.B2C, new List<string> { Role.Administrator });
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }
    }
}
