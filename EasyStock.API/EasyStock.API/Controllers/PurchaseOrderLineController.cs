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
        private readonly IExportService<OutputPurchaseOrderLineOverviewDto> _exportService;

        public PurchaseOrderLineController(IService<PurchaseOrderLine> service, IMapper mapper, IPurchaseOrderLineService purchaseOrderLineService, IExportService<OutputPurchaseOrderLineOverviewDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _purchaseOrderLineService = purchaseOrderLineService;
            _exportService = exportService;
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

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputPurchaseOrderLineOverviewDto>>(result.Data);

            var title = "PurchaseOrderLines";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputPurchaseOrderLineColumnDto.Columns, title);

            if (dto.Format == "csv")
            {
                contentType = "text/csv";
                fileName += ".csv";
            }
            else if (dto.Format == "excel")
            {
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName += ".xlsx";
            }
            else
            {
                return BadRequest("Unsupported export format.");
            }

            return File(file, contentType, fileName);

        }

    }
}