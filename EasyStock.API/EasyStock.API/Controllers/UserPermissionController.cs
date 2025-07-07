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
    [Route("api/userpermissions")]
    public class UserPermissionController : ControllerBase
    {
        private readonly IService<UserPermission> _service;
        private readonly IMapper _mapper;
        private readonly IUserPermissionService _userPermissionService;

        public UserPermissionController(IService<UserPermission> service, IMapper mapper, IUserPermissionService userPermissionService)
        {
            _service = service;
            _mapper = mapper;
            _userPermissionService = userPermissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserPermission>>> GetAll()
        {
            var entities = await _userPermissionService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputUserPermissionOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<UserPermission?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputUserPermissionDetailDto>(entity);
            dto.User = _mapper.Map<OutputUserOverviewDto>(entity.User);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateUserPermissionDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<UserPermission>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputUserPermissionDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateUserPermissionDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<UserPermission>(dto);
            await _service.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputUserPermissionOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _userPermissionService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputUserPermissionOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputUserPermissionOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}
