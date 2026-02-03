using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : AssetFoxCoreBaseController
    {
        public const string AnnouncementError = "Announcement Error";
        public AnnouncementController(IEsecSecurity esecSecurity, IUnitOfWork unitOfWork,
            IHubService hubService, IHttpContextAccessor httpContextAccessor) : base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("GetAnnouncements")]
        [ClaimAuthorize("AnnouncementViewAccess")]
        public async Task<IActionResult> Announcements()
        {
            try
            {
                var result = await Task.Factory.StartNew(() => UnitOfWork.AnnouncementRepo.Announcements());
                return Ok(result);
            }
            catch (Exception e)
            {
                var t = e.InnerException;
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AnnouncementError}::GetAnnouncements - {e.Message}", e);
                return Ok();
            }
        }

        [HttpPost]
        [Route("UpsertAnnouncement")]
        [ClaimAuthorize("AnnouncementModifyAccess")]
        public async Task<IActionResult> UpsertAnnouncement(AnnouncementDTO dto)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.AnnouncementRepo.UpsertAnnouncement(dto);
                });


                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name,$"{AnnouncementError}::UpsertAnnouncement - {e.Message}", e);
                return Ok();
            }
        }

        [HttpDelete]
        [Route("DeleteAnnouncement/{announcementId}")]
        [ClaimAuthorize("AnnouncementModifyAccess")]
        public async Task<IActionResult> DeleteAnnouncement(Guid announcementId)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.AnnouncementRepo.DeleteAnnouncement(announcementId);
                });

                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{AnnouncementError}::DeleteAnnouncement - {e.Message}", e);
                return Ok();
            }
        }
    }
}
