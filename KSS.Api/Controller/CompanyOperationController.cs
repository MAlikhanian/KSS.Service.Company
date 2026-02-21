using Microsoft.AspNetCore.Mvc;
using KSS.Dto;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/CompanyOperation")]
    [HasPermission("Company.Create")]
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
            var result = await _operationService.CreateCompanyWithTranslationsAndNameHistoryAsync(dto);
            return Ok(result);
        }
    }
}
