using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AssetFoxCore.Security;
using Humanizer;
using System.Data;
using System.Drawing;
using static AssetFoxCore.Security.SecurityConstants;
using System.IO;
using NetTopologySuite.Operation.Buffer;

namespace AssetFoxCore.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminSiteSettingsController : AssetFoxCoreBaseController
    {
        public const string AdminSiteSettingsError = "Admin Site Settings Error";

        public AdminSiteSettingsController(IEsecSecurity esecSecurity, IUnitOfWork unitOfWork, IHubService hubService, IHttpContextAccessor contextAccessor):
                         base(esecSecurity, unitOfWork, hubService, contextAccessor) { }

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
        [Route("GetImplementationLogo")]
        public async Task<IActionResult> GetImplementationLogo()
        {
            try
            {
                var result = await Task.Factory.StartNew(() => UnitOfWork.AdminSettingsRepo.GetImplementationLogo());
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::GetImplementationLogo - {e.Message}", e);
                return Ok();
            }
        }

        [HttpPost]
        [Route("SetImplementationLogo")]
        [Authorize(Policy = Policy.ModifyAdminSiteSettings)]
        public async Task<IActionResult> SetImplementationLogo()
        {
            try
            {
                var files = HttpContext.Request.Form.Files[0];
                byte[] buffer;
                if (!ContextAccessor.HttpContext.Request.HasFormContentType)
                    throw new ConstraintException("Request MIME type is invalid.");
                if (ContextAccessor.HttpContext.Request.Form.Files.Count < 1)
                    throw new ConstraintException("Attributes file not found.");
                if (files.ContentType == "image/svg+xml")
                {
                    using (var MemoryStream = new MemoryStream())
                    {
                        await
                        ContextAccessor.HttpContext.Request.Form.Files[0].CopyToAsync(MemoryStream);
                        buffer = MemoryStream.ToArray();
                    }

                    await Task.Factory.StartNew(() => UnitOfWork.AdminSettingsRepo.SetImplementationLogo(buffer));
                }      
                else
                {
                    Image logo = Image.FromStream(ContextAccessor.HttpContext.Request.Form.Files[0].OpenReadStream());
                    await Task.Factory.StartNew(() => UnitOfWork.AdminSettingsRepo.SetImplementationLogo(logo,files.ContentType.ToString()));
                }
                HubService.SendRealTimeMessage(UserInfo.Name, HubConstant.BroadcastTaskCompleted, "Successfully Updated Implementation Logo");
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AdminSiteSettingsError}::SetImplementationLogo - {e.Message}", e);
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
