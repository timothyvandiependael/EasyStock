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
    [Route("api/SalesOrders")]
    public class SalesOrderController : ControllerBase
    {
        private readonly IService<SalesOrder> _service;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IMapper _mapper;
        private readonly IExportService<OutputSalesOrderOverviewDto> _exportService;

        public SalesOrderController(IService<SalesOrder> service, IMapper mapper, ISalesOrderService salesOrderService, IExportService<OutputSalesOrderOverviewDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _salesOrderService = salesOrderService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll()
        {
            var entities = await _salesOrderService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputSalesOrderOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<SalesOrder?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputSalesOrderDetailDto>(entity);
            dto.Client = _mapper.Map<OutputClientDto>(entity.Client);
            dto.Lines = _mapper.Map<List<OutputSalesOrderLineOverviewDto>>(entity.Lines);


            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputSalesOrderColumnDto.Columns);
        }

        [PermissionAuthorize("SalesOrder", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateSalesOrderDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<SalesOrder>(dto);
            entity.Lines = _mapper.Map<List<SalesOrderLine>>(dto.Lines);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputSalesOrderDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("SalesOrder", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesOrderDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<SalesOrder>(dto);
            await _service.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [PermissionAuthorize("SalesOrder", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("SalesOrder", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputSalesOrderOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _salesOrderService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputSalesOrderOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputSalesOrderOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost("getproductswithsuppliersfororder")]
        public async Task<ActionResult<List<OutputProductDetailDto>>> GetProductsWithSuppliersForOrder(int id)
        {
            var products = await _salesOrderService.GetProductsWithSuppliersForOrderAsync(id);

            var productSuppliers = _mapper.Map<List<OutputProductDetailDto>>(products);
            
            return Ok(productSuppliers);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputSalesOrderOverviewDto>>(result.Data);

            var title = "SalesOrders";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputSalesOrderColumnDto.Columns, title);

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
