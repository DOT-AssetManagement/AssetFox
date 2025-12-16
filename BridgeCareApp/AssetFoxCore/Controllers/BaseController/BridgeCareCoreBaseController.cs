using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Models;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AssetFoxCore.Controllers.BaseController
{
    public class AssetFoxCoreBaseController : ControllerBase
    {
        public const string AssetFoxCoreBaseError = "AssetFoxCoreBase Error";

        protected readonly IEsecSecurity EsecSecurity;

        protected readonly IUnitOfWork UnitOfWork;

        protected readonly IHubService HubService;

        protected readonly IHttpContextAccessor ContextAccessor;

        private readonly IReadOnlyCollection<string> PathsToIgnore = new List<string>
        {
            "UserTokens", "RevokeToken", "RefreshToken"
        };

        public AssetFoxCoreBaseController(IEsecSecurity esecSecurity, IUnitOfWork unitOfWork, IHubService hubService, IHttpContextAccessor contextAccessor)
        {
            EsecSecurity = esecSecurity ?? throw new ArgumentNullException(nameof(esecSecurity));
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            HubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
            ContextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            if (RequestHasBearer())
            {
                SetUserInfo(ContextAccessor?.HttpContext?.Request);
            }
        }

        public bool RequestHasBearer()
        {
            if (ContextAccessor?.HttpContext?.Request != null)
            {
                return !PathsToIgnore.Any(pathToIgnore =>
                    ContextAccessor.HttpContext.Request.Path.Value.Contains(pathToIgnore));
            }

            return false;
        }

        public void SetUserInfo(HttpRequest request)
        {
            try
            {
                UserInfo = EsecSecurity.GetUserInformation(request);
            }
            catch(SecurityTokenExpiredException)
            {
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastWarning, $"{AssetFoxCoreBaseError}::The token is expired. Please re-login by pressing the button \"Go to login page\".");
                throw;
            }
            catch (Exception exception)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AssetFoxCoreBaseError}::SetUserInfo - {exception.Message}", exception);
            }
        }

        private UserInfo _userInfo;
        protected UserInfo UserInfo
        {
            get => _userInfo ?? new UserInfo();
            private set
            {
                if (_userInfo != value)
                {
                    _userInfo = value;
                    CheckIfUserExist();

                }
            }
        }

        private void CheckIfUserExist()
        {
            if (!string.IsNullOrEmpty(UserInfo.Name))
            {
                if (!UnitOfWork.UserRepo.UserExists(UserInfo.Name))
                {
                    UnitOfWork.AddUser(UserInfo.Name, UserInfo.HasAdminAccess);
                }

                UnitOfWork.SetUser(_userInfo.Name);
            }
        }
    }
}
