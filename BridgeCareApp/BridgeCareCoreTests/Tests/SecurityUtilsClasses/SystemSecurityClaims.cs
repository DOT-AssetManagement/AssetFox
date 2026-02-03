using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using AssetFoxCore.Security;
using SystemSecurityClaim = System.Security.Claims.Claim;

namespace AssetFoxCoreTests.Tests.SecurityUtilsClasses
{
    public static class SystemSecurityClaims
    {
        public static SystemSecurityClaim Admin()
        {
            var claim = new SystemSecurityClaim(ClaimTypes.Name, SecurityConstants.Claim.AdminAccess);
            return claim;
        }
    }
}
