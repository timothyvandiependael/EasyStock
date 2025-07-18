﻿using AutoMapper;
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
    [Route("api/dispatchlines")]
    public class DispatchLineController : ControllerBase
    {
        private readonly IService<DispatchLine> _service;
        private readonly IMapper _mapper;
        private readonly IDispatchLineService _dispatchLineService;

        public DispatchLineController(IService<DispatchLine> service, IMapper mapper, IDispatchLineService dispatchLineService)
        {
            _service = service;
            _mapper = mapper;
            _dispatchLineService = dispatchLineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DispatchLine>>> GetAll()
        {
            var entities = await _dispatchLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputDispatchLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<DispatchLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputDispatchLineDetailDto>(entity);
            dto.Dispatch = _mapper.Map<OutputDispatchOverviewDto>(entity.Dispatch);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputDispatchLineColumnDto.Columns);
        }

        [PermissionAuthorize("DispatchLine", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateDispatchLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<DispatchLine>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputDispatchLineDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("DispatchLine", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateDispatchLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<DispatchLine>(dto);
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

        [PermissionAuthorize("DispatchLine", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("DispatchLine", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputDispatchLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _dispatchLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputDispatchLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputDispatchLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}
