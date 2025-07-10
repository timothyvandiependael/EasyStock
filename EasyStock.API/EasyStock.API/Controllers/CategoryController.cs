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

        public CategoryController(IService<Category> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
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
