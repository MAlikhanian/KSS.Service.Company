using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KSS.Dto;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
    [HasPermission("Company.Read")]
    public class CompanyDetailController : ControllerBase
    {
        private readonly ICompanyDetailService _service;

        public CompanyDetailController(ICompanyDetailService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDetailDto>> GetById(Guid id, [FromQuery] short languageId = 12)
        {
            var result = await _service.GetByIdAsync(id, languageId);
            if (result == null)
                return NotFound(new { message = "Company not found." });
            return Ok(result);
        }

        [HttpPut("{id}")]
        [HasPermission("Company.Update")]
        public async Task<ActionResult<CompanyDetailDto>> Update(Guid id, [FromBody] CompanyDetailDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
