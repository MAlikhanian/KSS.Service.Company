using Microsoft.AspNetCore.Mvc;
using KSS.Dto;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/CompanyOperation")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CompanyOperationController : ControllerBase
    {
        private readonly ICompanyOperationService _operationService;

        public CompanyOperationController(ICompanyOperationService operationService)
        {
            _operationService = operationService;
        }

        [HttpPost("Insert")]
        public async Task<ActionResult<CompanyDto>> Insert([FromBody] CompanyInsertDto dto)
        {
            try
            {
                var result = await _operationService.CreateCompanyWithTranslationsAndNameHistoryAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the company", error = ex.Message });
            }
        }
    }
}
