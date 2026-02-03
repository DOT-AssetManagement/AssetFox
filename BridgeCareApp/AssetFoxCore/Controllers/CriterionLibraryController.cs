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

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CriterionLibraryController : AssetFoxCoreBaseController
    {
        public const string CriterionLibraryError = "Criterion Library Error";
        public CriterionLibraryController(IEsecSecurity esecSecurity, IUnitOfWork unitOfWork, IHubService hubService,
            IHttpContextAccessor httpContextAccessor) : base(esecSecurity, unitOfWork, hubService, httpContextAccessor) { }

        [HttpGet]
        [Route("GetCriterionLibraries")]
        [Authorize]
        public async Task<IActionResult> CriterionLibraries()
        {
            try
            {
                var result = await UnitOfWork.CriterionLibraryRepo.CriterionLibraries();
                return Ok(result);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name,  $"{CriterionLibraryError}::CriterionLibraries - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetSpecificCriteria/{libraryId}")]
        [Authorize]
        public async Task<IActionResult> GetCriteriaForScenario(Guid libraryId)
        {
            var result = await UnitOfWork.CriterionLibraryRepo.CriteriaLibrary(libraryId);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpsertCriterionLibrary")]
        [Authorize]
        public async Task<IActionResult> UpsertCriterionLibrary([FromBody] CriterionLibraryDTO dto)
        {
            try
            {
                var criterionLibraryId = Guid.Empty;
                await Task.Factory.StartNew(() =>
                {
                    criterionLibraryId = UnitOfWork.CriterionLibraryRepo.UpsertCriterionLibrary(dto);
                });

                return Ok(criterionLibraryId);
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{CriterionLibraryError}::UpsertCriterionLibrary {dto.MergedCriteriaExpression} - {e.Message}", e);
            }
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteCriterionLibrary/{libraryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCriterionLibrary(Guid libraryId)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    UnitOfWork.CriterionLibraryRepo.DeleteCriterionLibrary(libraryId);
                });

                return Ok();
            }
            catch (Exception e)
            {
                HubService.SendRealTimeErrorMessage(UserInfo.Name, $"{CriterionLibraryError}::DeleteCriterionLibrary - {e.Message}", e);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetHasPermittedAccess")]
        [Authorize]
        //[Authorize(Policy = Policy.ModifyFromCriterionLibrary)]
        public async Task<IActionResult> GetHasPermittedAccess()
        {
            return Ok(true);
        }
    }
}
