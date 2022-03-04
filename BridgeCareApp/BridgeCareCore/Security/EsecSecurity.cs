using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using BridgeCareCore.Models;
using BridgeCareCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace BridgeCareCore.Security
{
    public class EsecSecurity : IEsecSecurity
    {
        private readonly RsaSecurityKey _esecPublicKey;
        private readonly string _securityType;
        private readonly IConfiguration _config;

        /// <summary>
        ///     Each key is a token that has been revoked. Its value is the unix timestamp of the
        ///     time at which it expires.
        /// </summary>
        private ConcurrentDictionary<string, long> _revokedTokens;
        private Dictionary<string, string> userInformationDictionary;

        public EsecSecurity(IConfiguration config)
        {
            _revokedTokens = new ConcurrentDictionary<string, long>();
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityType = _config.GetSection("SecurityType").Value;
            _esecPublicKey = SecurityFunctions.GetPublicKey(_config.GetSection("EsecConfig"));

            userInformationDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Prevents the parser from accepting the provided token in the future.
        /// </summary>
        /// <param name="idToken">The JWT ID Token</param>
        /// For now, client app is not passing id token. So, this function is not in use
        public void RevokeToken(string idToken)
        {
            RemoveExpiredTokens();
            var decodedToken = DecodeToken(idToken);
            var expirationString = decodedToken.GetClaimValue("exp");
            var expiration = long.Parse(expirationString);
            _revokedTokens.TryAdd(idToken, expiration);
        }

        /// <summary>
        ///     Removes all expired tokens from the revokedTokens dictionary, as they no longer need
        ///     to be tracked. This keeps the dictionary from endlessly growing as the application runs
        /// </summary>
        private void RemoveExpiredTokens()
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _revokedTokens =
                new ConcurrentDictionary<string, long>(_revokedTokens.Where(entry => entry.Value > currentTime));
        }

        /// <summary>
        ///     Given an id_token from ESEC, validates it and extracts the User's Information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public UserInfo GetUserInformation(HttpRequest request)
        {
            var accessToken = request.Headers["Authorization"].ToString().Split(" ")[1];

            if (userInformationDictionary.Count == 0)
            {
                userInformationDictionary = GetUserInfoDictionary(accessToken);
            }

            if (!userInformationDictionary.ContainsKey("roles"))
            {
                throw new UnauthorizedAccessException("User has no roles assigned.");
            }

            var userInformation = GetUserInformation(userInformationDictionary);

            if (_securityType == SecurityConstants.SecurityTypes.Esec)
            {

                return userInformation;
            }

            if (_securityType == SecurityConstants.SecurityTypes.B2C)
            {
                userInformation.Role = SecurityConstants.Role.BAMSAdmin;
                return userInformation;
            }

            return new UserInfo { Name = "", Role = "", Email = "" };
        }

        private static string GetUserInfoString(string token)
        {
            // These two lines should be removed as soon as the ESEC site's certificates start working
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var esecConfig = Startup.StaticConfig.GetSection("ESECConfig");
            using var client = new HttpClient(handler) { BaseAddress = new Uri(esecConfig["ESECBaseAddress"]) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("access_token", WebUtility.UrlDecode(token))
            };
            HttpContent content = new FormUrlEncodedContent(formData);

            var responseTask = client.PostAsync("userinfo", content);
            responseTask.Wait();

            return responseTask.Result.Content.ReadAsStringAsync().Result;
        }

        public Dictionary<string, string> GetUserInfoDictionary(string token)
        {
            var response = GetUserInfoString(token);
            ValidateResponse(response);
            return DictionaryFromJSON(response);
        }
        private static Dictionary<string, string> DictionaryFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        }
        private void ValidateResponse(string response)
        {
            var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            if (!responseJson.ContainsKey("error"))
            {
                return;
            }
            throw new AuthenticationException(responseJson["error_description"]);
        }

        /// <summary>
        ///     Given a dictionary version of the LDAP-formatted JSON from ESEC, produces a UserInfo
        ///     object containing only the user's name, email, and relevant role
        /// </summary>
        /// <param name="userInformationDictionary"></param>
        /// <returns></returns>
        public UserInfo GetUserInformation(Dictionary<string, string> userInformationDictionary)
        {
            var role = SecurityFunctions.ParseLdap(userInformationDictionary["roles"])
                .First(roleString => Role.AllValidRoles.Contains(roleString));
            var name = SecurityFunctions.ParseLdap(userInformationDictionary["sub"])[0];
            var email = userInformationDictionary.ContainsKey("email") ? userInformationDictionary["email"] : null;
            return new UserInfo { Name = name, Role = role, Email = email };
        }

        /// <summary>
        ///     Creates a JwtSecurityToken object from a JWT string.
        /// </summary>
        /// <param name="idToken">JWT string</param>
        private JwtSecurityToken DecodeToken(string idToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true
            };

            var handler = new JwtSecurityTokenHandler();

            if (_securityType == SecurityConstants.SecurityTypes.Esec)
            {
                validationParameters.IssuerSigningKey = _esecPublicKey;

                handler.ValidateToken(idToken, validationParameters, out var validatedToken);
                return validatedToken as JwtSecurityToken;
            }

            var token = handler.ReadJwtToken(idToken);
            return token;
        }
    }
}
