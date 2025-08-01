﻿using Microsoft.AspNetCore.Mvc;
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
    [Route("api/StockMovements")]
    public class StockMovementController : ControllerBase
    {
        private readonly IService<StockMovement> _service;
        private readonly IMapper _mapper;
        private readonly IStockMovementService _stockMovementService;
        private readonly IExportService<OutputStockMovementOverviewDto> _exportService;

        public StockMovementController(IService<StockMovement> service, IMapper mapper, IStockMovementService stockMovementService, IExportService<OutputStockMovementOverviewDto> exportService)
        {
            _service = service;
            _mapper = mapper;
            _stockMovementService = stockMovementService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockMovement>>> GetAll()
        {
            var entities = await _stockMovementService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputStockMovementOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<StockMovement?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputStockMovementDetailDto>(entity);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputStockMovementColumnDto.Columns);
        }

        [PermissionAuthorize("StockMovement", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateStockMovementDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<StockMovement>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputStockMovementDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("StockMovement", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateStockMovementDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<StockMovement>(dto);
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

        [PermissionAuthorize("StockMovement", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("StockMovement", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputStockMovementOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _stockMovementService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputStockMovementOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputStockMovementOverviewDto>
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
            var dtoItems = _mapper.Map<List<OutputStockMovementOverviewDto>>(result.Data);

            var title = "StockMovements";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputStockMovementColumnDto.Columns, title);

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