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
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IService<Product> _service;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(IService<Product> service, IMapper mapper, IProductService productService)
        {
            _service = service;
            _mapper = mapper;
            _productService = productService;
        }

        [HttpGet]
        public  async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var entities = await _productService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputProductOverviewDto>>(entities);

            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Product?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputProductDetailDto>(entity);
            dto.AutoRestockSupplier = entity.AutoRestockSupplier == null ? null! : _mapper.Map<OutputSupplierOverviewDto>(entity.AutoRestockSupplier);
            dto.Category = entity.Category == null ? null! : _mapper.Map<OutputCategoryDto>(entity.Category);
            dto.Suppliers = _mapper.Map<List<OutputSupplierOverviewDto>>(entity.Suppliers);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputProductColumnDto.Columns);
        }

        [PermissionAuthorize("Product", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateProductDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<Product>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputProductDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("Product", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<Product>(dto);
            await _productService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [PermissionAuthorize("Product", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("Product", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputProductOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _productService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputProductOverviewDto>>(result.Data);
            return Ok(new PaginationResult<OutputProductOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

    }
}