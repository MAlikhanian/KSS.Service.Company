using Microsoft.AspNetCore.Mvc;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    /// <summary>
    /// ManagementController for company name operations that span multiple tables.
    /// Uses CompanyNameManagementService for cross-table orchestration.
    /// </summary>
    [ApiController]
    [Route("Api/CompanyNameManagement")]
    [PermissionGroup("Company")]
    public class CompanyNameManagementController : ControllerBase
    {
        private readonly ICompanyNameManagementService _managementService;

        public CompanyNameManagementController(ICompanyNameManagementService managementService)
        {
            _managementService = managementService;
        }

        /// <summary>
        /// Add a new name history entry with all translations.
        /// Backend handles: closing previous current entry, syncing to CompanyTranslation.
        /// </summary>
        [HttpPost("AddNameWithTranslations")]
        [HasPermission("Company.Create")]
        public async Task<ActionResult> AddNameWithTranslations([FromBody] AddNameWithTranslationsDto dto)
        {
            var result = await _managementService.AddNameWithTranslationsAsync(dto);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return Ok(dto);
        }

        /// <summary>
        /// Upsert translations for an existing name history entry.
        /// Backend handles: add vs update per language, syncing to CompanyTranslation.
        /// </summary>
        [HttpPut("UpsertTranslations")]
        [HasPermission("Company.Update")]
        public async Task<ActionResult> UpsertTranslations([FromBody] UpsertNameTranslationsDto dto)
        {
            await _managementService.UpsertTranslationsAsync(dto);
            return Ok(dto);
        }

        /// <summary>
        /// Delete a name history entry with business rule validation.
        /// Rules: only newest, not first, not the only entry.
        /// </summary>
        [HttpDelete("DeleteNameHistory")]
        [HasPermission("Company.Delete")]
        public ActionResult DeleteNameHistory([FromBody] CompanyNameHistory item)
        {
            var result = _managementService.DeleteNameHistory(item);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return NoContent();
        }

        /// <summary>
        /// Remove a single translation from a name history entry.
        /// Backend handles: last-translation protection, CompanyTranslation sync.
        /// </summary>
        [HttpDelete("RemoveTranslation")]
        [HasPermission("Company.Delete")]
        public ActionResult RemoveTranslation([FromBody] RemoveTranslationDto dto)
        {
            var result = _managementService.RemoveTranslation(dto);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return NoContent();
        }
    }
}
