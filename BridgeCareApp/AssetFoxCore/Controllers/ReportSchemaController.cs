using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFoxCore.Controllers.BaseController;
using AssetFoxCore.Security.Interfaces;
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
            IHttpContextAccessor httpContextAccessor)
            : base(esecSecurity, unitOfWork, hubService, httpContextAccessor)
        {
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            try
            {
                var schemas = UnitOfWork.ReportSchemaRepo.GetAll();
                return Ok(schemas);
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
                var schema = UnitOfWork.ReportSchemaRepo.Get(id);
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

                UnitOfWork.ReportSchemaRepo.Upsert(dto);
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
                dto.ModifiedDate = DateTime.UtcNow;
                UnitOfWork.ReportSchemaRepo.Upsert(dto);
                return Ok(dto);
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

                UnitOfWork.ReportSchemaRepo.Delete(id);
                return Ok();
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
