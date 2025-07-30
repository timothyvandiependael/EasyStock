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
    [Route("api/PurchaseOrders")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IService<PurchaseOrder> _service;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IMapper _mapper;
        private readonly IExportService<OutputPurchaseOrderOverviewDto> _exportService;

        public PurchaseOrderController(IService<PurchaseOrder> service, IMapper mapper, IPurchaseOrderService purchaseOrderService, IExportService<OutputPurchaseOrderOverviewDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _purchaseOrderService = purchaseOrderService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetAll()
        {
            var entities = await _purchaseOrderService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputPurchaseOrderOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<PurchaseOrder?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputPurchaseOrderDetailDto>(entity);
            dto.Supplier = _mapper.Map<OutputSupplierOverviewDto>(entity.Supplier);
            dto.Lines = _mapper.Map<List<OutputPurchaseOrderLineOverviewDto>>(entity.Lines);


            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputPurchaseOrderColumnDto.Columns);
        }

        [PermissionAuthorize("PurchaseOrder", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreatePurchaseOrderDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<PurchaseOrder>(dto);
            entity.Lines = _mapper.Map<List<PurchaseOrderLine>>(dto.Lines);
            await _purchaseOrderService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            OutputPurchaseOrderDetailDto resultDto = null;
            try
            {
                resultDto = _mapper.Map<OutputPurchaseOrderDetailDto>(entity);
                return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }
            
        }

        [PermissionAuthorize("PurchaseOrder", "add")]
        [HttpPost("fromsalesorder")]
        public async Task<ActionResult> AddFromSalesOrder([FromBody] CreatePurchaseOrderFromSalesOrderDto dto)
        {
            if (dto == null) return BadRequest();
            var resultList = await _purchaseOrderService.AddFromSalesOrder(dto.SalesOrderId, dto.ProductSuppliers, HttpContext.User.Identity!.Name!);

            var resultDtoList = new List<OutputPurchaseOrderDetailDto>();
            foreach (var result in resultList)
            {
                var resultDto = _mapper.Map<OutputPurchaseOrderDetailDto>(result);
                resultDto.Lines = _mapper.Map<List<OutputPurchaseOrderLineOverviewDto>>(result.Lines);
                resultDtoList.Add(resultDto);
            }

            return Ok(resultDtoList);
        }

        [PermissionAuthorize("PurchaseOrder", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<PurchaseOrder>(dto);
            await _service.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _purchaseOrderService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("PurchaseOrder", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _purchaseOrderService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("PurchaseOrder", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _purchaseOrderService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputPurchaseOrderOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _purchaseOrderService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputPurchaseOrderOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputPurchaseOrderOverviewDto>
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
            var dtoItems = _mapper.Map<List<OutputPurchaseOrderOverviewDto>>(result.Data);

            var title = "PurchaseOrders";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputPurchaseOrderColumnDto.Columns, title);

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
