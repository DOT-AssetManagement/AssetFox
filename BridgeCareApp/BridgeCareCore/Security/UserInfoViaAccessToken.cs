using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore;
using BridgeCareCore.Models;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;

namespace BridgeCareCore.Security
{
    public static class UserInfoViaAccessToken
    {
        public static Dictionary<string, string> GetUserInfoDictionary(string token)
        {
            var response = GetUserInfoString(token);
            ValidateResponse(response);
            return DictionaryFromJSON(response);
        }
        public static UserInfo GetUserInformation(Dictionary<string, string> userInformationDictionary)
        {
            var role = SecurityFunctions.ParseLdap(userInformationDictionary["roles"])
                .First(roleString => Role.AllValidRoles.Contains(roleString));
            var name = SecurityFunctions.ParseLdap(userInformationDictionary["sub"])[0];
            var email = userInformationDictionary.ContainsKey("email") ? userInformationDictionary["email"] : null;
            return new UserInfo { Name = name, Role = role, Email = email };
        }


        private static Dictionary<string, string> DictionaryFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
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
        private static void ValidateResponse(string response)
        {
            var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            if (!responseJson.ContainsKey("error"))
            {
                return;
            }
            throw new AuthenticationException(responseJson["error_description"]);
        }
    }
}
