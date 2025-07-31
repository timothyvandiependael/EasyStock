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
    [Route("api/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IService<Client> _service;
        private readonly IMapper _mapper;
        private readonly IExportService<OutputClientOverviewDto> _exportService;
        private readonly IClientService _clientService;

        public ClientController(IService<Client> service, IMapper mapper, IExportService<OutputClientOverviewDto> exportService, IClientService clientService)
        {
            _service = service;
            _mapper = mapper;
            _exportService = exportService;
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll()
        {
            var entities = await _service.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputClientOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Client?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputClientDetailDto>(entity);
            dto.SalesOrders = _mapper.Map<List<OutputSalesOrderOverviewDto>>(dto.SalesOrders);
            return Ok(dto);
        }

        [HttpGet("columns")]
        public ActionResult<List<ColumnMetaData>> GetColumns()
        {
            return Ok(OutputClientColumnDto.Columns);
        }

        [PermissionAuthorize("Client", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateClientDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<Client>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputClientDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("Client", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateClientDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<Client>(dto);
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

        [PermissionAuthorize("Client", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("Client", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputClientOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _clientService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputClientOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputClientOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportAdvanced([FromBody] ExportRequestDto dto)
        {
            if (dto.Parameters == null || dto.Parameters.Filters == null || dto.Parameters.Sorting == null || string.IsNullOrEmpty(dto.Format)) return BadRequest("Missing parameters");

            var result = await _clientService.GetAdvancedAsync(dto.Parameters.Filters, dto.Parameters.Sorting, null);
            var dtoItems = _mapper.Map<List<OutputClientOverviewDto>>(result.Data);

            var title = "Clients";
            var contentType = "";
            var fileName = title + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var file = _exportService.GenerateExport(dtoItems, dto.Format, OutputClientColumnDto.Columns, title);

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