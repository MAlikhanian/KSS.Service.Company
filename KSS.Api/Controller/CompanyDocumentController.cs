using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class CompanyDocumentController : BaseController<CompanyDocument, CompanyDocumentViewDto, CompanyDocumentInsertDto, CompanyDocumentUpdateDto>
    {
        private readonly ICompanyDocumentService _companyDocumentService;

        public CompanyDocumentController(ICompanyDocumentService service) : base(service)
        {
            _companyDocumentService = service;
        }

        /// <summary>
        /// GET /Api/CompanyDocument/ByCompany/{companyId} — a company's document
        /// metadata rows (newest first). The `[action]` route token supplies the
        /// "ByCompany" segment, so the template is just "{companyId}".
        /// </summary>
        [HttpGet("{companyId}")]
        public async Task<ActionResult> ByCompany(Guid companyId)
            => Ok(await _companyDocumentService.GetByCompanyAsync(companyId));

        /// <summary>
        /// POST /Api/CompanyDocument/Create — create the metadata row from an
        /// InsertDto (no nav props to trip implicit-required validation) and return
        /// the created row WITH its backend-stamped GUID id. The BFF needs that id
        /// to upload the file bytes through the orchestrator.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CompanyDocumentInsertDto dto)
            => Ok(await _companyDocumentService.CreateAsync(dto));

        /// <summary>
        /// DELETE /Api/CompanyDocument/ById/{id} — delete a document by key only
        /// (matches the "delete takes only the id" convention; avoids binding a
        /// full entity whose non-nullable nav props would be implicitly required).
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult ById(Guid id)
        {
            _companyDocumentService.DeleteById(id);
            return NoContent();
        }
    }
}
