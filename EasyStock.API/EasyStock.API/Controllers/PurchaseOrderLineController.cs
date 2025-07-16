using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using EasyStock.API.Common;
using EasyStock.API.Dtos;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/PurchaseOrderLines")]
    public class PurchaseOrderLineController : ControllerBase
    {
        private readonly IService<PurchaseOrderLine> _service;
        private readonly IMapper _mapper;
        private readonly IPurchaseOrderLineService _purchaseOrderLineService;

        public PurchaseOrderLineController(IService<PurchaseOrderLine> service, IMapper mapper, IPurchaseOrderLineService purchaseOrderLineService)
        {
            _service = service;
            _mapper = mapper;
            _purchaseOrderLineService = purchaseOrderLineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrderLine>>> GetAll()
        {
            var entities = await _purchaseOrderLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputPurchaseOrderLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<PurchaseOrderLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputPurchaseOrderLineDetailDto>(entity);
            dto.PurchaseOrder = _mapper.Map<OutputPurchaseOrderOverviewDto>(entity.PurchaseOrder);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            dto.ReceptionLines = entity.ReceptionLines == null 
                ? new List<OutputReceptionLineOverviewDto>()
                : _mapper.Map<List<OutputReceptionLineOverviewDto>>(entity.ReceptionLines);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputPurchaseOrderLineColumnDto.Columns);
        }

        [PermissionAuthorize("PurchaseOrderLine", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreatePurchaseOrderLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<PurchaseOrderLine>(dto);
            await _purchaseOrderLineService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputPurchaseOrderLineDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("PurchaseOrderLine", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdatePurchaseOrderLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<PurchaseOrderLine>(dto);
            await _purchaseOrderLineService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {

            await _purchaseOrderLineService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("PurchaseOrderLine", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _purchaseOrderLineService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("PurchaseOrderLine", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _purchaseOrderLineService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputPurchaseOrderLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _purchaseOrderLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputPurchaseOrderLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputPurchaseOrderLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

    }
}