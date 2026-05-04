using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Enums;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Models;
using AssetFoxCore.Security.Interfaces;
using AssetFoxCore.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetFoxCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportSchemaController : AssetFoxCoreBaseController
    {
        public ReportSchemaController(
            IEsecSecurity esecSecurity,
            IUnitOfWork unitOfWork,
            IHubService hubService,
            IHttpContextAccessor httpContextAccessor,
            IClaimHelper claimHelper)
            : base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
            _claimHelper = claimHelper ?? throw new ArgumentNullException(nameof(claimHelper));
        }

        private readonly IClaimHelper _claimHelper;
        private Guid UserId => UnitOfWork.CurrentUser?.Id ?? Guid.Empty;
        private bool IncludeAllTemplates => !_claimHelper.RequirePermittedCheck();

        public class ReportSchemaSharingRequest
        {
            public bool IsSharedWithAll { get; set; }
            public List<LibraryUserDTO> Users { get; set; } = new List<LibraryUserDTO>();
        }

        public class ReportSchemaPagingRequest : PagingRequestModel<ReportSchemaDTO>
        {
            public string Scope { get; set; } = "all";
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            try
            {
                var schemas = UnitOfWork.ReportSchemaRepo.GetAccessibleSummaries(UserId, IncludeAllTemplates);
                return Ok(schemas);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpPost("page")]
        [Authorize]
        public IActionResult GetPage([FromBody] ReportSchemaPagingRequest request)
        {
            try
            {
                var schemas = UnitOfWork.ReportSchemaRepo.GetAccessibleSummaries(UserId, IncludeAllTemplates);
                return Ok(PageSchemas(schemas, request));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            try
            {
                var schema = UnitOfWork.ReportSchemaRepo.GetAccessible(id, UserId, IncludeAllTemplates);
                if (schema == null)
                {
                    return NotFound();
                }
                return Ok(schema);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] ReportSchemaDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request body is required.");
                }

                // Validate schema JSON
                if (!string.IsNullOrWhiteSpace(dto.SchemaJson))
                {
                    if (!SchemaParser.TryParse(dto.SchemaJson, out var schema, out var error))
                    {
                        return BadRequest($"Invalid schema JSON: {error}");
                    }

                    if (!ValidateSchemaIds(schema, out error))
                    {
                        return BadRequest($"Invalid schema JSON: {error}");
                    }
                }

                if (dto.Id == Guid.Empty)
                {
                    dto.Id = Guid.NewGuid();
                }

                dto.CreatedDate = DateTime.UtcNow;
                dto.ModifiedDate = DateTime.UtcNow;
                dto.CreatedByName = UnitOfWork.CurrentUser?.Username ?? "system";
                if (_claimHelper.RequirePermittedCheck())
                {
                    dto.IsSystemDefault = false;
                }

                UnitOfWork.ReportSchemaRepo.Upsert(dto, UserId);
                dto = UnitOfWork.ReportSchemaRepo.GetAccessible(dto.Id, UserId, IncludeAllTemplates);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] ReportSchemaDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request body is required.");
                }

                var existing = UnitOfWork.ReportSchemaRepo.Get(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var access = UnitOfWork.ReportSchemaRepo.GetLibraryAccess(id, UserId);
                _claimHelper.CheckUserLibraryModifyAuthorization(access, UserId);

                // Validate schema JSON
                if (!string.IsNullOrWhiteSpace(dto.SchemaJson))
                {
                    if (!SchemaParser.TryParse(dto.SchemaJson, out var schema, out var error))
                    {
                        return BadRequest($"Invalid schema JSON: {error}");
                    }

                    if (!ValidateSchemaIds(schema, out error))
                    {
                        return BadRequest($"Invalid schema JSON: {error}");
                    }
                }

                dto.Id = id;
                dto.IsSharedWithAll = existing.IsSharedWithAll;
                if (_claimHelper.RequirePermittedCheck())
                {
                    dto.IsSystemDefault = existing.IsSystemDefault;
                }
                dto.ModifiedDate = DateTime.UtcNow;
                UnitOfWork.ReportSchemaRepo.Upsert(dto, UserId);
                var updated = UnitOfWork.ReportSchemaRepo.GetAccessible(id, UserId, IncludeAllTemplates);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var existing = UnitOfWork.ReportSchemaRepo.Get(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var access = UnitOfWork.ReportSchemaRepo.GetLibraryAccess(id, UserId);
                _claimHelper.CheckUserLibraryDeleteAuthorization(access, UserId);
                UnitOfWork.ReportSchemaRepo.Delete(id);
                return Ok();
            }
            catch (UnauthorizedAccessException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpGet("defaults")]
        [Authorize]
        public IActionResult GetDefaults()
        {
            try
            {
                var schemas = UnitOfWork.ReportSchemaRepo.GetDefaults();
                return Ok(schemas);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpGet("{id}/users")]
        [Authorize]
        public IActionResult GetUsers(Guid id)
        {
            try
            {
                var access = UnitOfWork.ReportSchemaRepo.GetLibraryAccess(id, UserId);
                _claimHelper.CheckGetLibraryUsersValidity(access, UserId);
                var users = UnitOfWork.ReportSchemaRepo.GetLibraryUsers(id);
                return Ok(users);
            }
            catch (UnauthorizedAccessException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        [HttpPut("{id}/sharing")]
        [Authorize]
        public IActionResult UpdateSharing(Guid id, [FromBody] ReportSchemaSharingRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request body is required.");
                }

                var existing = UnitOfWork.ReportSchemaRepo.Get(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var usersBefore = UnitOfWork.ReportSchemaRepo.GetLibraryUsers(id);
                _claimHelper.CheckAccessModifyValidity(usersBefore, request.Users ?? new List<LibraryUserDTO>(), UserId);

                UnitOfWork.ReportSchemaRepo.UpsertOrDeleteUsers(id, request.Users ?? new List<LibraryUserDTO>());
                existing.IsSharedWithAll = request.IsSharedWithAll;
                UnitOfWork.ReportSchemaRepo.Upsert(existing, UserId);

                var updated = UnitOfWork.ReportSchemaRepo.GetAccessible(id, UserId, IncludeAllTemplates);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = e.Message });
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(new { error = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.Message });
            }
        }

        private static PagingPageModel<ReportSchemaDTO> PageSchemas(
            List<ReportSchemaDTO> schemas,
            ReportSchemaPagingRequest request)
        {
            request ??= new ReportSchemaPagingRequest();
            schemas = ApplyScope(schemas, request.Scope);

            if (!string.IsNullOrWhiteSpace(request.search))
            {
                var search = request.search.Trim().ToLowerInvariant();
                schemas = schemas.Where(_ =>
                    (_.Name?.ToLowerInvariant().Contains(search) ?? false) ||
                    (_.Description?.ToLowerInvariant().Contains(search) ?? false) ||
                    (_.ReportCategory?.ToLowerInvariant().Contains(search) ?? false) ||
                    (_.OwnerName?.ToLowerInvariant().Contains(search) ?? false) ||
                    (_.CreatedByName?.ToLowerInvariant().Contains(search) ?? false))
                    .ToList();
            }

            schemas = OrderSchemas(schemas, request.sortColumn, request.isDescending);
            var total = schemas.Count;
            if (request.RowsPerPage > 0)
            {
                schemas = schemas
                    .Skip(request.RowsPerPage * Math.Max(request.Page - 1, 0))
                    .Take(request.RowsPerPage)
                    .ToList();
            }

            return new PagingPageModel<ReportSchemaDTO>
            {
                Items = schemas,
                TotalItems = total
            };
        }

        private static List<ReportSchemaDTO> ApplyScope(List<ReportSchemaDTO> schemas, string scope)
        {
            return (scope ?? "all").ToLowerInvariant() switch
            {
                "mine" => schemas
                    .Where(_ => !_.IsSystemDefault && _.CurrentUserAccessLevel == LibraryAccessLevel.Owner)
                    .ToList(),
                "shared" => schemas
                    .Where(_ =>
                        !_.IsSystemDefault &&
                        (_.IsSharedWithAll ||
                         _.IsShared ||
                         (_.CurrentUserAccessLevel.HasValue && _.CurrentUserAccessLevel != LibraryAccessLevel.Owner)))
                    .ToList(),
                "system" => schemas.Where(_ => _.IsSystemDefault).ToList(),
                _ => schemas,
            };
        }

        private static List<ReportSchemaDTO> OrderSchemas(
            List<ReportSchemaDTO> schemas,
            string sortColumn,
            bool isDescending)
        {
            sortColumn = sortColumn?.ToLowerInvariant();
            return sortColumn switch
            {
                "name" => isDescending
                    ? schemas.OrderByDescending(_ => _.Name).ToList()
                    : schemas.OrderBy(_ => _.Name).ToList(),
                "ownername" or "owner" => isDescending
                    ? schemas.OrderByDescending(_ => _.OwnerName).ToList()
                    : schemas.OrderBy(_ => _.OwnerName).ToList(),
                "reportcategory" or "category" => isDescending
                    ? schemas.OrderByDescending(_ => _.ReportCategory).ToList()
                    : schemas.OrderBy(_ => _.ReportCategory).ToList(),
                "modifieddate" => isDescending
                    ? schemas.OrderByDescending(_ => _.ModifiedDate).ToList()
                    : schemas.OrderBy(_ => _.ModifiedDate).ToList(),
                "createddate" => isDescending
                    ? schemas.OrderByDescending(_ => _.CreatedDate).ToList()
                    : schemas.OrderBy(_ => _.CreatedDate).ToList(),
                _ => schemas.OrderByDescending(_ => _.ModifiedDate).ToList(),
            };
        }

        private static bool ValidateSchemaIds(ReportSchemaDefinition schema, out string error)
        {
            error = null;

            foreach (var page in schema.Pages ?? new List<ReportPage>())
            {
                if (!Guid.TryParse(page.Id, out _))
                {
                    error = "Every report page id must be a GUID.";
                    return false;
                }

                foreach (var component in page.Components ?? new List<ReportComponent>())
                {
                    if (!Guid.TryParse(component.Id, out _))
                    {
                        error = "Every report component id must be a GUID.";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
