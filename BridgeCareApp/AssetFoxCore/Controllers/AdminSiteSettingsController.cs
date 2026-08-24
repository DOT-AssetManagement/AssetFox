using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AssetFoxCore.Security;
using static AssetFoxCore.Security.SecurityConstants;

namespace AssetFoxCore.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminSiteSettingsController : AssetFoxCoreBaseController
    {
        public const string AdminSiteSettingsError = "Admin Site Settings Error";
        public AdminSiteSettingsController(
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor contextAccessor) :
            base(esecSecurity, unitOfWork, hubService, contextAccessor)
        { }

        [HttpGet]
        [Route("GetImplementationName")]
        public async Task<IActionResult> GetImplementationName()
        {
            try
            {
                var name = UnitOfWork.AdminSettingsRepo.GetImplementationName();
                return Ok(name);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::GetImplementationName - {e.Message}", e);
                return Ok();
            }
        }

        [HttpPost]
        [Route("SetImplementationName/{name}")]
        [Authorize(Policy = Policy.ModifyAdminSiteSettings)]
        public async Task<IActionResult> SetImplementationName(string name)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.AdminSettingsRepo.SetImplementationName(name);
                });
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastTaskCompleted, "Successfully Updated Implementation Name");
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::SetImplementationName - {e.Message}", e);
                return Ok();
            }
        }

        [HttpGet]
        [Route("GetAdminContactEmail")]
        public async Task<IActionResult> GetAdminContactEmail()
        {
            try
            {
                var email = UnitOfWork.AdminSettingsRepo.GetAdminContactEmail();
                return Ok(email);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::GetAdminContactEmail - {e.Message}", e);
                return Ok();
            }
        }

        [HttpPost]
        [Route("SetAdminContactEmail/{email}")]
        [Authorize(Policy = Policy.ModifyAdminSiteSettings)]
        public async Task<IActionResult> SetAdminContactEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email cannot be empty");
            }

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.AdminSettingsRepo.SetAdminContactEmail(email);
                });
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastTaskCompleted, "Successfully Updated Admin Contact Email");
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::SetAdminContactEmail - {e.Message}", e);
                return Ok();
            }
        }

    }
}
