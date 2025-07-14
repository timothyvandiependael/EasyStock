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
    [Route("api/dispatches")]
    public class DispatchController : ControllerBase
    {
        private readonly IService<Dispatch> _service;
        private readonly IDispatchService _dispatchService;
        private readonly IMapper _mapper;
        private readonly ISalesOrderService _salesOrderService;

        public DispatchController(IService<Dispatch> service, IMapper mapper, IDispatchService DispatchService, ISalesOrderService salesOrderService)
        {
            _service = service;
            _mapper = mapper;
            _dispatchService = DispatchService;
            _salesOrderService = salesOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dispatch>>> GetAll()
        {
            var entities = await _dispatchService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputDispatchOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Dispatch?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputDispatchDetailDto>(entity);
            dto.Client = _mapper.Map<OutputClientDto>(entity.Client);
            dto.Lines = _mapper.Map<List<OutputDispatchLineOverviewDto>>(entity.Lines);


            return Ok(dto);
        }

        [PermissionAuthorize("Dispatch", "add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateDispatchDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<Dispatch>(dto);
            entity.Lines = _mapper.Map<List<DispatchLine>>(dto.Lines);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputDispatchDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [PermissionAuthorize("Dispatch", "add")]
        [HttpPost("fromsalesorder")]
        public async Task<ActionResult> AddFromSalesOrder([FromBody] CreateDispatchFromSalesOrderDto dto)
        {
            if (dto == null) return BadRequest();

            var isComplete = await _salesOrderService.IsComplete(dto.SalesOrderId);
            if (!isComplete) return BadRequest($"Cannot make dispatch: Sales order with id {dto.SalesOrderId} is not complete.");

            var result = await _dispatchService.AddFromSalesOrder(dto.SalesOrderId, HttpContext.User.Identity!.Name!);
            if (result == null) throw new Exception($"Unable to create dispatch based on sales order with id {dto.SalesOrderId}");
            var resultDto = _mapper.Map<OutputDispatchDetailDto>(result);
            resultDto.Lines = _mapper.Map<List<OutputDispatchLineOverviewDto>>(result.Lines);
            return Ok(resultDto);
        }

        [PermissionAuthorize("Dispatch", "edit")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateDispatchDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<Dispatch>(dto);
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

        [PermissionAuthorize("Dispatch", "delete")]
        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _service.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [PermissionAuthorize("Dispatch", "delete")]
        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _service.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputDispatchOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _dispatchService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputDispatchOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputDispatchOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}
