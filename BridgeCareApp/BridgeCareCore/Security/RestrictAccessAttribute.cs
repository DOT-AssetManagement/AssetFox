using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using BridgeCareCore.Models;
using BridgeCareCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BridgeCareCore.Security
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RestrictAccessAttribute : Attribute, IAuthorizationFilter
    {
        private readonly Func<string, bool> ValidateRole;

        /// <summary>
        ///     Only users with the provided roles will be able to access the endpoint. If no roles
        ///     are listed, all authenticated users will be able to access it.
        /// </summary>
        /// <param name="roles">Permitted roles</param>
        public RestrictAccessAttribute(params string[] roles) : base()
        {
            ValidateRole = role => roles.Contains(role);
        }

        public RestrictAccessAttribute() : base()
        {
            ValidateRole = role => true;
        }

        /// <summary>
        ///     Attempts to get an authorization parameter from an HTTP request's headers
        /// </summary>
        /// <param name="headers">Request headers</param>
        /// <returns>Returns true if successful</returns>
        private (bool, UserInfo) TryGetAuthorization(HttpRequest request, out string authorization)
        {
            var data = request.Headers["Authorization"].ToString().Split(" ");
            if (data.Length <= 1)
            {
                authorization = "";
                return (false, new UserInfo());
            }

            if (data[0] == "BearerB2C")
            {
                var decodedToken = DecodeToken(data[1]);
                authorization = data[1];
                var info = new UserInfo
                {
                    Name = decodedToken.GetClaimValue("name"),
                    Email = decodedToken.GetClaimValue("email"),
                    Role = SecurityConstants.Role.BAMSAdmin
                };
                return (true, info);
            }
            authorization = data[1];

            try
            {
                var userInformationDictionary = UserInfoViaAccessToken.GetUserInfoDictionary(authorization);
                if (userInformationDictionary.ContainsKey("error"))
                {
                    return (false, new UserInfo());
                }
                var userInformation = UserInfoViaAccessToken.GetUserInformation(userInformationDictionary);
                return (ValidateRole(userInformation.Role), userInformation);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException(ex.Message);
            }
        }

        private JwtSecurityToken DecodeToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(idToken);
            return token;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            try
            {
                var data = TryGetAuthorization(request, out string accessToken);
                if (!data.Item1)
                {
                    context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
                else
                {
                    context.HttpContext.Items.Add("name", data.Item2.Name);
                    context.HttpContext.Items.Add("role", data.Item2.Role);
                    context.HttpContext.Items.Add("email", data.Item2.Email);
                }
            }
            catch
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }

            // Some API endpoints need this user information, so it is inserted into
            // the request here before they process it
        }
    }
}
