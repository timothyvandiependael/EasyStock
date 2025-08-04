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
    [Route("api/SalesOrderLines")]
    public class SalesOrderLineController : ControllerBase
    {
        private readonly IService<SalesOrderLine> _service;
        private readonly IMapper _mapper;
        private readonly ISalesOrderLineService _salesOrderLineService;
        private readonly IProductService _productService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IExportService<OutputSalesOrderLineOverviewDto> _exportService;
        private readonly IService<Product> _genericProductService;

        public SalesOrderLineController(IService<SalesOrderLine> service, IMapper mapper, ISalesOrderLineService salesOrderLineService, IProductService productService, IPurchaseOrderService purchaseOrderService, IExportService<OutputSalesOrderLineOverviewDto> exportService, IService<Product> genericProductService)
        {
            _service = service;
            _mapper = mapper;
            _salesOrderLineService = salesOrderLineService;
            _productService = productService;
            _purchaseOrderService = purchaseOrderService;
            _exportService = exportService;
            _genericProductService = genericProductService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderLine>>> GetAll()
        {
            var entities = await _salesOrderLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputSalesOrderLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<SalesOrderLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputSalesOrderLineDetailDto>(entity);
            dto.SalesOrder = _mapper.Map<OutputSalesOrderOverviewDto>(entity.SalesOrder);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputSalesOrderLineColumnDto.Columns);
        }

        [PermissionAuthorize("SalesOrderLine", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateSalesOrderLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<SalesOrderLine>(dto);
           

            var resultDto = await _salesOrderLineService.AddAsync(entity, HttpContext.User.Identity!.Name!);


            return Ok(resultDto);
        }

        [PermissionAuthorize("SalesOrderLine", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesOrderLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<SalesOrderLine>(dto);
            await _salesOrderLineService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = new AutoRestockDto();
            resultDto.AutoRestocked = false;
            var product = await _genericProductService.GetByIdAsync(entity.ProductId);
            if (product == null) throw new Exception("Error while finding product for autorestock.");
            resultDto.ProductName = product.Name;
            var isBelowMinimumStock = await _productService.IsProductBelowMinimumStock(entity.ProductId);
            var productSurplus = await _productService.GetProductSurplusAfterReceptions(entity.ProductId);
            if (isBelowMinimumStock)
            {
                if (productSurplus < 0)
                {
                    if (entity.Product.AutoRestock)
                    {
                        var po = await _purchaseOrderService.AutoRestockProduct(entity.ProductId, HttpContext.User.Identity!.Name!);
                        if (po == null)
                            throw new Exception("Error while creating purchase order for autorestock.");
                        
                        resultDto.AutoRestockPurchaseOrderNumber = po.OrderNumber;
                        resultDto.AutoRestocked = true;
                    }
                    else
                    {
                        resultDto.ProductShortage = Math.Abs(productSurplus);
                    }
                }
            }
            else
            {
                if (productSurplus < 0)
                {
                    resultDto.ProductShortage = Math.Abs(productSurplus);
                }
            }

            return Ok(resultDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _salesOrderLineService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("SalesOrderLine", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _salesOrderLineService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("SalesOrderLine", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _salesOrderLineService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputSalesOrderLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _salesOrderLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputSalesOrderLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputSalesOrderLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _salesOrderLineService.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputSalesOrderLineOverviewDto>>(result.Data);

            var title = "SalesOrderLines";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputSalesOrderLineColumnDto.Columns, title);

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
