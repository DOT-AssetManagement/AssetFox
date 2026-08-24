using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AssetFoxCore.Security
{
    public static class SecurityFunctions
    {
        public static UserInfoDTO ToDto(this UserInfo userInfo) =>
            new UserInfoDTO { Sub = userInfo.Name, HasAdminAccess  = userInfo.HasAdminAccess, HasSimulationAccess = userInfo.HasSimulationAccess, Email = userInfo.Email };

        /// <summary>
        ///     Retrieves the value of the claim of the given type from the JWT payload claims.
        /// </summary>
        /// <returns></returns>
        public static string GetClaimValue(this JwtSecurityToken jwt, string type) =>
            jwt.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;

        /// <summary>
        ///     Derives the stable, unique username for an Entra External ID user from its token.
        ///     The display-name ("name") claim is NOT unique, so it cannot be used as an identity
        ///     key: two users with the same display name would collapse into a single account and
        ///     see each other's data. This prefers the email, falling back to preferred_username and
        ///     finally the immutable directory object id (oid) so that a usable unique key is always
        ///     produced. This must stay in lock-step with the frontend's username derivation and the
        ///     value read from the same access token, or SignalR routing and share-by-username break.
        /// </summary>
        public static string GetEntraUsername(this JwtSecurityToken jwt) =>
            FirstNonEmpty(
                jwt.GetClaimValue("email"),
                jwt.GetClaimValue("preferred_username"),
                jwt.GetClaimValue("oid"),
                jwt.GetClaimValue("http://schemas.microsoft.com/identity/claims/objectidentifier"));

        private static string FirstNonEmpty(params string[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        /// <summary>
        ///     Given an LDAP-formatted string from ESEC, extracts the Common Name (CN) fields.
        /// </summary>
        /// <param name="ldap"></param>
        /// <returns>Role</returns>
        public static List<string> ParseLdap(string ldap)
        {
            if (string.IsNullOrEmpty(ldap))
            {
                return new List<string>();
            }

            var segments = ldap.Split('^');
            var commonNameSegments = segments.Select(segment => segment.Split(',')[0]);
            var commonNames = commonNameSegments.Select(segment => segment.Split('=')[1]);
            return commonNames.ToList();
        }

        /// <summary>
        ///     Fetches the public key information from the ESEC jwks endpoint, and generates an
        ///     RsaSecurityKey from it
        /// </summary>
        public static RsaSecurityKey GetPublicKey(IConfiguration esecConfig)
        {
            // These two lines should be removed as soon as the ESEC site's certificates start working
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var baseAddress = esecConfig["EsecBaseAddress"];
            using var client = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var responseTask = client.GetAsync("jwks?AuthorizationProvider=" + esecConfig["EsecOIDCProvider"]);
            responseTask.Wait();

            var resultJson = responseTask.Result.Content.ReadAsStringAsync().Result;

            // This dictionary and list structure matches the structure of the JSON response from
            // the ESEC JWKS endpoint
            var resultDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(resultJson);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(new RSAParameters()
            {
                Modulus = Base64UrlEncoder.DecodeBytes(resultDictionary["keys"][0]["n"]),
                Exponent = Base64UrlEncoder.DecodeBytes(resultDictionary["keys"][0]["e"])
            });
            return new RsaSecurityKey(rsa);
        }
    }
}
