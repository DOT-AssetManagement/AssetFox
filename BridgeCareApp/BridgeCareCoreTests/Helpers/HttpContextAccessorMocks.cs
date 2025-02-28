using System.Collections.Generic;
using System.Security.Claims;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCoreTests.Helpers;
using BridgeCareCoreTests.Tests.SecurityUtilsClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace BridgeCareCoreTests
{
    // This class contains some hasty merges. It may need refactoring.
    public static class HttpContextAccessorMocks
    {
        public static Mock<IHttpContextAccessor> DefaultMock(
            List<Claim> claims = null,
            Dictionary<string, StringValues> queryStore = null)
        {
            var mock = new Mock<IHttpContextAccessor>();
            var context = HttpContextSetup.WithAuthorizationHeader(queryStore);
            if (claims != null)
            {
                var claimsPrincipal = ClaimsPrincipals.WithClaims(claims);
                context.User = claimsPrincipal;
            }
            mock.Setup(_ => _.HttpContext).Returns(context);
            return mock;
        }

        public static Mock<IHttpContextAccessor> MockWithAdminClaims(
            Dictionary<string, StringValues> queryStore = null)
        {
            var claims = SystemSecurityClaimLists.Admin();
            var mock = DefaultMock(claims, queryStore);
            return mock;
        }

        public static Mock<IHttpContextAccessor> AdminWithFormCollection(IFormCollection requestFormCollection)
        {
            var mock = MockWithAdminClaims();
            var httpContext = mock.Object.HttpContext;
            httpContext.Request.Form = requestFormCollection;
            return mock;
        }

        public static IHttpContextAccessor Default(List<Claim> claims = null, Dictionary<string, StringValues> queryStore = null)
        {
            var mock = DefaultMock(claims, queryStore);
            return mock.Object;
        }

        public static IHttpContextAccessor Admin(Dictionary<string, StringValues> queryStore = null)
        {
            var mock = MockWithAdminClaims(queryStore);
            return mock.Object;
        }

        public static void AddClaims(this Mock<IHttpContextAccessor> mock, List<Claim> claims)
        {
            var context = new DefaultHttpContext();
            HttpContextSetup.AddAuthorizationHeader(context);

            var claimsPrincipal = ClaimsPrincipals.WithClaims(claims);
            context.User = claimsPrincipal;

            mock.Setup(_ => _.HttpContext).Returns(context);
        }

        public static IHttpContextAccessor WithClaims(List<Claim> claims)
        {
            var mock = MockWithClaims(claims);
            return mock.Object;
        }

        public static Mock<IHttpContextAccessor> MockWithClaims(List<Claim> claims)
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.AddClaims(claims);
            return mock;
        }
    }
}
