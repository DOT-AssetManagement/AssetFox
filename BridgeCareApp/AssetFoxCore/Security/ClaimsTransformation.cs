using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using AssetFoxCore.Utils.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using AssetFoxCore.Security.Interfaces;
using AssetFox.Core.Common;

namespace AssetFoxCore.Security
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        private readonly IConfiguration _config;
        private readonly IRoleClaimsMapper _roleClaimsMapper;
        private readonly IGraphApiClientService _graphApiClientService;
        private Dictionary<string, UserCache> cache = new();
        private readonly ILog _log;

        public ClaimsTransformation(IConfiguration config, IRoleClaimsMapper roleClaimsMapper, IGraphApiClientService graphApiClientService, ILog log)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _roleClaimsMapper = roleClaimsMapper ?? throw new ArgumentNullException(nameof(roleClaimsMapper));
            _graphApiClientService = graphApiClientService ?? throw new ArgumentNullException(nameof(graphApiClientService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (_config.GetSection("SecurityType").Value == SecurityConstants.SecurityTypes.Esec)
            {
                // Obtain the role(s) from the claims principal
                var roleClaim = principal.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Role).Value;
                var rolesParsed = new List<string>();
                if (roleClaim != null)
                {
                    rolesParsed = SecurityFunctions.ParseLdap(roleClaim);
                }
                if (rolesParsed.Count == 0)
                {
                    rolesParsed.Add(SecurityConstants.Role.Default);
                }

                var internalRolesFromMapper = _roleClaimsMapper.GetInternalRoles(SecurityConstants.SecurityTypes.Esec, rolesParsed);
                var claimsFromMapper = _roleClaimsMapper.GetClaims(SecurityConstants.SecurityTypes.Esec, internalRolesFromMapper);
                var identity = _roleClaimsMapper.AddClaimsToUserIdentity(principal, internalRolesFromMapper, claimsFromMapper);
                principal.AddIdentity(identity);
            }

            var securityType = _config.GetSection("SecurityType").Value;
            if (securityType == SecurityConstants.SecurityTypes.B2C
                || securityType == SecurityConstants.SecurityTypes.Entra)
            {
                string cacheKey;
                if (securityType == SecurityConstants.SecurityTypes.Entra)
                {
                    // Entra External ID: use the directory object identifier (oid) as the
                    // Graph lookup / cache key. The "sub"/nameidentifier claim is a pairwise
                    // pseudonymous id that is unique per application and is NOT accepted by
                    // Microsoft Graph's /users/{id} endpoint; only the oid (a directory GUID)
                    // is. The claim is "oid" when inbound claim mapping is disabled and the
                    // long schema URI when it is enabled, so accept either form.
                    var objectIdClaim = principal.Claims.FirstOrDefault(t =>
                        t.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier"
                        || t.Type == "oid");
                    cacheKey = objectIdClaim?.Value;
                }
                else
                {
                    // Azure AD B2C: the nameidentifier claim is the Graph lookup / cache key.
                    var nameidentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                    var nameidentifier = principal.Claims.FirstOrDefault(t => t.Type == nameidentifierClaimType);
                    cacheKey = nameidentifier?.Value;
                }
                lock (cache)
                {
                    try
                    {
                        if (cacheKey != null && !cache.ContainsKey(cacheKey) || cache[cacheKey].LastRefreshTime < DateTime.Now.AddDays(-1))
                        {
                            // Read group name(s) set in Azure B2C(that will be IP role name(s)
                            var groupNames = new List<string>();
                            var groupClaimType = "group";
                            if (!principal.HasClaim(claim => claim.Type == groupClaimType))
                            {                                
                                groupNames = _graphApiClientService.GetGraphApiUserMemberGroup(cacheKey)?.Result;
                            }
                            if (groupNames.Count == 0)
                            {
                                groupNames.Add(SecurityConstants.Role.Default);
                            }

                            var internalRolesFromMapper = _roleClaimsMapper.GetInternalRoles(securityType, groupNames);
                            var claimsFromMapper = _roleClaimsMapper.GetClaims(securityType, internalRolesFromMapper);
                            var identity = _roleClaimsMapper.AddClaimsToUserIdentity(principal, internalRolesFromMapper, claimsFromMapper);

                            if (!cache.ContainsKey(cacheKey))
                            {
                                cache.Add(cacheKey, new UserCache { Identity = identity, LastRefreshTime = DateTime.Now });
                            }
                            else
                            {
                                if (cache[cacheKey].LastRefreshTime < DateTime.Now.AddDays(-1))
                                {
                                    cache[cacheKey].Identity = identity;
                                    cache[cacheKey].LastRefreshTime = DateTime.Now;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Ignore: control gets here few times due to any concurrent calls. Solution to avoid any exception can be looked at later.
                        // Log the full exception (message + inner exceptions), not just the stack trace —
                        // a faulted Graph call wraps the real cause (e.g. 403 consent, 404 unknown oid)
                        // which is otherwise invisible.
                        _log.Error("ClaimsTransformation Error: " + e);
                    }
                }
                // Only add the cached identity if it was actually populated above. A failed
                // Graph lookup is swallowed in the catch block, leaving no cache entry;
                // indexing the dictionary blindly here would surface that as an opaque
                // KeyNotFoundException (HTTP 500) instead of the user simply having no
                // mapped roles.
                if (cacheKey != null && cache.TryGetValue(cacheKey, out var userCache))
                {
                    principal.AddIdentity(userCache.Identity);
                }
            }
            return principal;
        }
    }    
}
