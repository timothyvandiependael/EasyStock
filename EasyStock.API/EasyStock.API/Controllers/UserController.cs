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
        private readonly IAuthService _authService;
        private readonly IExportService<OutputUserOverviewDto> _exportService;
        private readonly IUserService _userService;

        public UserController(IService<User> service, IMapper mapper, IAuthService authService, IExportService<OutputUserOverviewDto> exportService, IUserService userService)
        {
            _service = service;
            _mapper = mapper;
            _authService = authService;
            _exportService = exportService;
            _userService = userService;
        }

        [PermissionAuthorize("User", "view")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var entities = await _service.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputUserOverviewDto>>(entities);
            return Ok(dtos);
        }

        [PermissionAuthorize("User", "view")]
        [HttpGet("id/{id}")]
        public async Task<ActionResult<User?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputUserDetailDto>(entity);

            dto.Role = (await _authService.GetRoleAsync(dto.UserName)).ToString();
            dto.Permissions = _mapper.Map<List<OutputUserPermissionOverviewDto>>(entity.Permissions);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputUserColumnDto.Columns);
        }

        [PermissionAuthorize("User", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateUserDto dto)
        {
            if (dto == null) return BadRequest();
            var userExists = await _authService.UserExists(dto.UserName);
            if (userExists)
            {
                ModelState.AddModelError("userName", "Username already exists.");
                return ValidationProblem(ModelState);
            }

            var entity = _mapper.Map<User>(dto);
            entity.Permissions = _mapper.Map<List<UserPermission>>(dto.Permissions);

            await _userService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var pw = await _authService.AddAsync(entity, Enum.Parse<UserRole>(dto.Role), HttpContext.User.Identity!.Name!);

            return Ok(new { password = pw });
        }

        [PermissionAuthorize("User", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<User>(dto);
            await _userService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);
            await _authService.UpdateRoleAsync(entity, Enum.Parse<UserRole>(dto.Role), HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [PermissionAuthorize("User", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _userService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("User", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _userService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("User", "view")]
        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputUserOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _service.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputUserOverviewDto>>(result.Data);

            var userRoles = await _authService.GetRolesAsync();

            foreach (var dto in dtoItems)
            {
                dto.Role = userRoles[dto.UserName].ToString();
            }

            return Ok(new PaginationResult<OutputUserOverviewDto>
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
            var dtoItems = _mapper.Map<List<OutputUserOverviewDto>>(result.Data);

            var title = "Users";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputUserColumnDto.Columns, title);

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