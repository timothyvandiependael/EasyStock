using AutoMapper;
using EasyStock.API.Common;
using EasyStock.API.Dtos;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/receptionlines")]
    public class ReceptionLineController : ControllerBase
    {
        private readonly IService<ReceptionLine> _service;
        private readonly IMapper _mapper;
        private readonly IReceptionLineService _receptionLineService;

        public ReceptionLineController(IService<ReceptionLine> service, IMapper mapper, IReceptionLineService receptionLineService)
        {
            _service = service;
            _mapper = mapper;
            _receptionLineService = receptionLineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceptionLine>>> GetAll()
        {
            var entities = await _receptionLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputReceptionLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<ReceptionLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputReceptionLineDetailDto>(entity);
            dto.Reception = _mapper.Map<OutputReceptionOverviewDto>(entity.Reception);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            dto.PurchaseOrderLine = _mapper.Map<OutputPurchaseOrderLineOverviewDto>(entity.PurchaseOrderLine);
            return Ok(dto);
        }

        [PermissionAuthorize("ReceptionLine", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateReceptionLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<ReceptionLine>(dto);
            await _receptionLineService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputReceptionLineDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("ReceptionLine", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateReceptionLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<ReceptionLine>(dto);
            await _receptionLineService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _receptionLineService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("ReceptionLine", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _receptionLineService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("ReceptionLine", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _receptionLineService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputReceptionLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _receptionLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputReceptionLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputReceptionLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}
