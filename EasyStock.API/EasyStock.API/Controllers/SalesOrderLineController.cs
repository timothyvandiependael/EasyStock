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
    [Route("api/SalesOrderLines")]
    public class SalesOrderLineController : ControllerBase
    {
        private readonly IService<SalesOrderLine> _service;
        private readonly IMapper _mapper;
        private readonly ISalesOrderLineService _salesOrderLineService;

        public SalesOrderLineController(IService<SalesOrderLine> service, IMapper mapper, ISalesOrderLineService salesOrderLineService)
        {
            _service = service;
            _mapper = mapper;
            _salesOrderLineService = salesOrderLineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderLine>>> GetAll()
        {
            var entities = await _salesOrderLineService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputSalesOrderLineOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<SalesOrderLine?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputSalesOrderLineDetailDto>(entity);
            dto.SalesOrder = _mapper.Map<OutputSalesOrderOverviewDto>(entity.SalesOrder);
            dto.Product = _mapper.Map<OutputProductOverviewDto>(entity.Product);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateSalesOrderLineDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<SalesOrderLine>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputSalesOrderLineDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateSalesOrderLineDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<SalesOrderLine>(dto);
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
        public async Task<ActionResult<PaginationResult<OutputSalesOrderLineOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null) return BadRequest("Missing parameters");

            var result = await _salesOrderLineService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputSalesOrderLineOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputSalesOrderLineOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

    }
}
