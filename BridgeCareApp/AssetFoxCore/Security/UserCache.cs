using System.Security.Claims;
using System;

namespace AssetFoxCore.Security;

public class UserCache
{
    public ClaimsIdentity Identity { get; set; }

    public DateTime LastRefreshTime { get; set; }
}
