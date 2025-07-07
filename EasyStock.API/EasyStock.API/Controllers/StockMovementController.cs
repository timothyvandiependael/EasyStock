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
    [Route("api/StockMovements")]
    public class StockMovementController : ControllerBase
    {
        private readonly IService<StockMovement> _service;
        private readonly IMapper _mapper;
        private readonly IStockMovementService _stockMovementService;

        public StockMovementController(IService<StockMovement> service, IMapper mapper, IStockMovementService stockMovementService)
        {
            _service = service;
            _mapper = mapper;
            _stockMovementService = stockMovementService;
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

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateStockMovementDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<StockMovement>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputStockMovementDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateStockMovementDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<StockMovement>(dto);
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
        public async Task<ActionResult<PaginationResult<OutputStockMovementOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null) return BadRequest("Missing parameters");

            var result = await _stockMovementService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputStockMovementOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputStockMovementOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }

    }
}