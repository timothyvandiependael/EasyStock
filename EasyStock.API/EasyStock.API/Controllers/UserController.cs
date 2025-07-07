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
    [Route("api/Users")]
    public class UserController : ControllerBase
    {
        private readonly IService<User> _service;
        private readonly IMapper _mapper;

        public UserController(IService<User> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var entities = await _service.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputUserOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<User?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputUserDetailDto>(entity);
            dto.Permissions = _mapper.Map<List<OutputUserPermissionOverviewDto>>(entity.Permissions);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateUserDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<User>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            // TODO : CUSTOM LOGIC FOR USER CREATION WITH PASSWORD, USER SERVICE?

            var resultDto = _mapper.Map<OutputUserDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<User>(dto);
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
        public async Task<ActionResult<PaginationResult<OutputUserOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputUserOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputUserOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}