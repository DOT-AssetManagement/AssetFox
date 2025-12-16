using System;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFoxCore.Controllers.BaseController;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFoxCore.Security;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Policy = AssetFoxCore.Security.SecurityConstants.Policy;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : AssetFoxCoreBaseController
    {
        public UserController(IEsecSecurity esecSecurity, UnitOfDataPersistenceWork unitOfWork, IHubService hubService,
            IHttpContextAccessor httpContextAccessor) : base(esecSecurity, unitOfWork, hubService, httpContextAccessor) { }

        [HttpGet]
        [Route("GetAllUsers")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await UnitOfWork.UserRepo.GetAllUsers();
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"User error::GetAllUsers - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetUserByUserName/{userName}")]
        [Authorize]
        public async Task<IActionResult> GetUserByUserName( string userName)
        {
            try
            {
                var result = await UnitOfWork.UserRepo.GetUserByUserName(userName);
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"User error::GetUserByUserName - {e.Message}", e);
                throw;
            }
            return Ok();
        }

        [HttpPost]
        [Route("UpdateLastNewsAccessDate")]
        [Authorize]
        public async Task<IActionResult> UpdateLastNewsAccessDate([FromBody] LastNewsAccessDateDTO dto)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.UserRepo.UpdateLastNewsAccessDate(dto.Id, dto.LastNewsAccessDate);
                });
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"User error::UpdateLastNewsAccessDate - {e.Message}", e);
            }
            return Ok();
        }

        [HttpPut]
        [Route("UpdateUser")]
        [ClaimAuthorize("AdminAccess")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO dto)
        {
            var username = dto?.Username ?? "null";
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"User error::UpdateUser {username} - {e.Message}", e);
            }
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteUser/{username}")]
        [ClaimAuthorize("AdminAccess")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"User error::DeleteUser {username} - {e.Message}", e);
            }
            return Ok();
        }
    }
}
