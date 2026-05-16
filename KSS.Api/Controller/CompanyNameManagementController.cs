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
    [PermissionGroup("Information")]
    public class CompanyNameManagementController : ControllerBase
    {
        private readonly ICompanyNameManagementService _managementService;

        public CompanyNameManagementController(ICompanyNameManagementService managementService)
        {
            _managementService = managementService;
        }

        /// <summary>
        /// Add a new name history entry with all translations.
        /// Backend handles: closing previous current entry, syncing to Translation.
        /// </summary>
        [HttpPost("AddNameWithTranslations")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult> AddNameWithTranslations([FromBody] AddNameWithTranslationsDto dto)
        {
            var result = await _managementService.AddNameWithTranslationsAsync(dto);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return Ok(dto);
        }

        /// <summary>
        /// Upsert translations for an existing name history entry.
        /// Backend handles: add vs update per language, syncing to Translation.
        /// </summary>
        [HttpPut("UpsertTranslations")]
        [HasPermission("Company.Information.Modify")]
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
        [HasPermission("Company.Information.Modify")]
        public ActionResult DeleteNameHistory([FromBody] NameHistoryDeleteDto dto)
        {
            var result = _managementService.DeleteNameHistory(dto.Id, dto.CompanyId);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return NoContent();
        }

        /// <summary>
        /// Remove a single translation from a name history entry.
        /// Backend handles: last-translation protection, Translation sync.
        /// </summary>
        [HttpDelete("RemoveTranslation")]
        [HasPermission("Company.Information.Modify")]
        public ActionResult RemoveTranslation([FromBody] RemoveTranslationDto dto)
        {
            var result = _managementService.RemoveTranslation(dto);
            if (!result.Success)
                return BadRequest(new { statusCode = 400, message = result.Message });

            return NoContent();
        }
    }
}
