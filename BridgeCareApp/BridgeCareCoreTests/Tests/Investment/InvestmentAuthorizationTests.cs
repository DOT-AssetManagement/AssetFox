using System.Security.Claims;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCore.Utils;
using BridgeCareCoreTests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BridgeCareCoreTests.Tests
{
    public class InvestmentAuthorizationTests
    {
        private const string PolicyName = "TestInvestmentPolicy";

        [Fact]
        public async Task UserIsViewInvestmentFromScenarioAuthorized()
        {
            // admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.InvestmentViewAnyFromScenarioAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var roles = new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Administrator };
            var claims = roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, roles);
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }

        [Fact]
        public async Task UserIsModifyInvestmentFromLibraryAuthorized()
        {
            // non-admin authorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.InvestmentModifyPermittedFromLibraryAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var roles = new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Editor };
            var claims = roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, roles);
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);

        }
        [Fact]
        public async Task UserIsImportInvestmentFromScenarioAuthorized()
        {
            // non-admin unauthorized
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.InvestmentImportPermittedFromScenarioAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();

            var roles = new List<string> { BridgeCareCore.Security.SecurityConstants.Role.ReadOnly };
            var claims = roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, roles);
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.False(allowed.Succeeded);

        }
        [Fact]
        public async Task UserIsViewInvestmentFromScenarioAuthorized_B2C()
        {
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(PolicyName,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.InvestmentViewAnyFromScenarioAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var roles = new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Administrator };
            var claims = roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.B2C, roles);
            var user = ClaimsPrincipals.WithNameClaims(claims);
            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, PolicyName);
            // Assert
            Assert.True(allowed.Succeeded);
        }
    }
}
