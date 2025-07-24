using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;
using EasyStock.API.Common;
using EasyStock.API.Dtos;
using AutoMapper;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly IService<Category> _service;
        private readonly IMapper _mapper;
        private readonly IExportService<OutputCategoryDto> _exportService;

        public CategoryController(IService<Category> service, IMapper mapper, IExportService<OutputCategoryDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll()
        {
            var entities = await _service.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputCategoryDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Category?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputCategoryDto>(entity);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputCategoryColumnDto.Columns);
        }

        [PermissionAuthorize("Category", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateCategoryDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<Category>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputCategoryDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("Category", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<Category>(dto);
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

        [PermissionAuthorize("Category", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("Category", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputCategoryDto>>(result.Data);

            var title = "Categories";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputCategoryColumnDto.Columns, title);

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

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputCategoryDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputCategoryDto>>(result.Data);

            return Ok(new PaginationResult<OutputCategoryDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

    }
}
