using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security.Interfaces;
using AssetFoxCore.Services.Branding;
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
        private readonly IClientLogoService _clientLogoService;

        public AdminSiteSettingsController(
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor contextAccessor,
            IClientLogoService clientLogoService) :
            base(esecSecurity, unitOfWork, hubService, contextAccessor)
        {
            _clientLogoService = clientLogoService ?? throw new ArgumentNullException(nameof(clientLogoService));
        }

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
        public Task<IActionResult> GetImplementationLogo(CancellationToken cancellationToken)
        {
            return GetLogo(ClientLogoKind.Implementation, "GetImplementationLogo", cancellationToken);
        }

        [HttpGet]
        [Route("GetAgencyLogo")]
        public Task<IActionResult> GetAgencyLogo(CancellationToken cancellationToken)
        {
            return GetLogo(ClientLogoKind.Agency, "GetAgencyLogo", cancellationToken);
        }

        [HttpPost]
        [Route("SetImplementationLogo")]
        [Authorize(Policy = Policy.ModifyAdminSiteSettings)]
        public Task<IActionResult> SetImplementationLogo(
            [FromForm] IFormFile file,
            CancellationToken cancellationToken)
        {
            return SetLogo(
                ClientLogoKind.Implementation,
                "Implementation",
                file,
                cancellationToken);
        }

        [HttpPost]
        [Route("SetAgencyLogo")]
        [Authorize(Policy = Policy.ModifyAdminSiteSettings)]
        public Task<IActionResult> SetAgencyLogo(
            [FromForm] IFormFile file,
            CancellationToken cancellationToken)
        {
            return SetLogo(ClientLogoKind.Agency, "Agency", file, cancellationToken);
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

        private async Task<IActionResult> GetLogo(
            ClientLogoKind logoKind,
            string operationName,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _clientLogoService.ReadAsDataUrlAsync(logoKind, cancellationToken);
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(
                    UserInfo.Name,
                    $"{AdminSiteSettingsError}::{operationName} - {e.Message}",
                    e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        private async Task<IActionResult> SetLogo(
            ClientLogoKind logoKind,
            string logoName,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            try
            {
                var fileName = await _clientLogoService.SaveAsync(logoKind, file, cancellationToken);
                HubService.SendRealTimeMessage(
                    UserInfo.Name,
                    HubConstant.BroadcastTaskCompleted,
                    $"Successfully Updated {logoName} Logo");
                return Ok(new { fileName });
            }
            catch (InvalidDataException e)
            {
                HubService.SendRealTimeErrorMessage(
                    UserInfo.Name,
                    $"{AdminSiteSettingsError}::Set{logoName}Logo - {e.Message}",
                    e);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(
                    UserInfo.Name,
                    $"{AdminSiteSettingsError}::Set{logoName}Logo - {e.Message}",
                    e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
