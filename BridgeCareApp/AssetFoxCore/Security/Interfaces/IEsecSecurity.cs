using System.Collections.Generic;
using AssetFoxCore.Models;
using Microsoft.AspNetCore.Http;

namespace AssetFoxCore.Security.Interfaces
{
    public interface IEsecSecurity
    {
        void RevokeToken(string idToken);

        UserInfo GetUserInformation(HttpRequest request);

        UserInfo GetUserInformation(Dictionary<string, string> userInformationDictionary);
    }
}
