using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KSS.Helper;
using KSS.Helper.Model;
using KSS.Helper.Authorization;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    [Authorize]
    [ServiceFilter(typeof(PermissionAuthorizationFilter))]
    public class BaseController<T, TViewDto, TAddDto, TUpdateDto> : ControllerBase
        where T : class
        where TViewDto : class
        where TAddDto : class
        where TUpdateDto : class
    {
        private readonly IBaseService<T, TViewDto, TAddDto, TUpdateDto> _service;
        public BaseController(IBaseService<T, TViewDto, TAddDto, TUpdateDto> service) => _service = service;

        [HttpPost]
        public async Task<ActionResult> FindAsync([FromBody] Filter id)
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(await _service.FindAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult> SingleAsync([FromBody] T filter)
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(await _service.SingleAsync(filter));
        }

        [HttpGet]
        public async Task<ActionResult> ToListAllAsync()
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(await _service.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult> ToListAsync([FromBody] T filter)
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(await _service.ToListAsync(filter));
        }

        [HttpPost]
        public async Task<ActionResult> ToListByFilterAsync([FromBody] Filter filter)
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(await _service.ToListAsync(filter));
        }

        [HttpPost]
        public async Task<ActionResult> ToListDtoAsync([FromBody] T filter)
        {
            if (RefuseUnscopedRead() is { } refused) return refused;
            return Ok(_service.Dto(await _service.ToListAsync(filter)));
        }

        [HttpPost]
        public async Task<ActionResult> AddAsync([FromBody] T item)
        {
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(new[] { item })) is { } refused) return refused;
            await _service.AddAsync(item);

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult> AddDtoAsync([FromBody] TAddDto item)
        {
            if (RefuseUnscopedWrite() is { } refused) return refused;
            await _service.AddDtoAsync(item);

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult> AddRangeAsync([FromBody] IEnumerable<T> items)
        {
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(items)) is { } refused) return refused;
            await _service.AddRangeAsync(items);

            return Ok(items);
        }

        [HttpPut]
        public IActionResult Update([FromBody] T item)
        {
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(new[] { item })) is { } refused) return refused;
            _service.Update(item);

            return NoContent();
        }

        [HttpPut]
        public IActionResult UpdateDto([FromBody] TUpdateDto item)
        {
            if (RefuseUnscopedWrite() is { } refused) return refused;
            _service.UpdateDto(item);

            return NoContent();
        }

        [HttpPut]
        public IActionResult UpdateRange([FromBody] IEnumerable<T> items)
        {
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(items)) is { } refused) return refused;
            _service.UpdateRange(items);

            return NoContent();
        }

        [HttpDelete()]
        public IActionResult Remove([FromBody] T item)
        {
            // Related records attached to a removal would be removed with it by cascade.
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(new[] { item })) is { } refused) return refused;
            _service.Remove(item);

            return NoContent();
        }

        [HttpDelete]
        public IActionResult RemoveRange([FromBody] IEnumerable<T> items)
        {
            if ((RefuseUnscopedWrite() ?? RefuseRelatedRecords(items)) is { } refused) return refused;
            _service.RemoveRange(items);

            return NoContent();
        }

        // The generic read actions are available only when the service declares
        // ICompanyScopedReads, meaning every read is limited to the companies the caller may
        // read, or IReferenceData, meaning its rows belong to no company. A permission is global
        // to the caller, so an unscoped read of company data would show every company's records
        // to any holder. This refusal is the control, not an oversight: a record type becomes
        // readable here only by scoping its service's reads, never by removing this line.
        private ActionResult? RefuseUnscopedRead()
        {
            if (_service is ICompanyScopedReads || _service is IReferenceData) return null;
            return StatusCode(403, new { statusCode = 403, message = "This record type cannot be read through this route." });
        }

        // The generic write actions are available only when the service declares
        // ICompanyScopedWrites, meaning every write checks the caller's company level on the
        // target and on the stored row. A permission is global to the caller, so a write without
        // that check would let a holder change any company's records, or estate-wide reference
        // data. This refusal is the control, not an oversight: a record type becomes writable
        // here only by adding the checks to its service, never by removing this line.
        private ActionResult? RefuseUnscopedWrite()
        {
            if (_service is ICompanyScopedWrites) return null;
            return StatusCode(403, new { statusCode = 403, message = "This record type cannot be changed through this route." });
        }

        // A write request carries the record itself and nothing reachable from it. Related
        // records in the body would be saved with it, into the company they name or moved into
        // this one, so a request that carries any is refused before it reaches the service.
        private ActionResult? RefuseRelatedRecords(IEnumerable<T?>? items)
        {
            var carried = (items ?? Array.Empty<T?>()).SelectMany(RelatedRecords.CarriedBy).Distinct().ToList();
            if (carried.Count == 0) return null;
            return BadRequest(new { statusCode = 400, message = "A write request cannot carry related records: " + string.Join(", ", carried) + "." });
        }
    }
}