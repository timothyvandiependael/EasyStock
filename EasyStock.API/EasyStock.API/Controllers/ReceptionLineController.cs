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
    [Route("api/receptionlines")]
    public class ReceptionLineController : ControllerBase
    {
        private readonly IService<ReceptionLine> _service;
        private readonly IMapper _mapper;
        private readonly IReceptionLineService _receptionLineService;
        private readonly IExportService<OutputReceptionLineOverviewDto> _exportService;

        public ReceptionLineController(IService<ReceptionLine> service, IMapper mapper, IReceptionLineService receptionLineService, IExportService<OutputReceptionLineOverviewDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _receptionLineService = receptionLineService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceptionLine>>> GetAll()
        {
            var entities = await _receptionLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputReceptionLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<ReceptionLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputReceptionLineDetailDto>(entity);
            dto.Reception = _mapper.Map<OutputReceptionOverviewDto>(entity.Reception);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            dto.PurchaseOrderLine = _mapper.Map<OutputPurchaseOrderLineOverviewDto>(entity.PurchaseOrderLine);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputReceptionLineColumnDto.Columns);
        }

        [PermissionAuthorize("ReceptionLine", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateReceptionLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<ReceptionLine>(dto);
            await _receptionLineService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputReceptionLineDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("ReceptionLine", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateReceptionLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<ReceptionLine>(dto);
            await _receptionLineService.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _receptionLineService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("ReceptionLine", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _receptionLineService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("ReceptionLine", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _receptionLineService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputReceptionLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _receptionLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputReceptionLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputReceptionLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _receptionLineService.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputReceptionLineOverviewDto>>(result.Data);

            var title = "ReceptionLines";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputReceptionLineColumnDto.Columns, title);

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
