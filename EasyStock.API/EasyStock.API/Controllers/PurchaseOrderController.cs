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
    [Route("api/PurchaseOrders")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IService<PurchaseOrder> _service;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IMapper _mapper;

        public PurchaseOrderController(IService<PurchaseOrder> service, IMapper mapper, IPurchaseOrderService purchaseOrderService)
        {
            _service = service;
            _mapper = mapper;
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetAll()
        {
            var entities = await _purchaseOrderService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputPurchaseOrderOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<PurchaseOrder?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputPurchaseOrderDetailDto>(entity);
            dto.Supplier = _mapper.Map<OutputSupplierDto>(entity.Supplier);
            dto.Lines = _mapper.Map<List<OutputPurchaseOrderLineOverviewDto>>(entity.Lines);


            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreatePurchaseOrderDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<PurchaseOrder>(dto);
            await _service.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputPurchaseOrderDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<PurchaseOrder>(dto);
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
        public async Task<ActionResult<PaginationResult<OutputPurchaseOrderOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null) return BadRequest("Missing parameters");

            var result = await _purchaseOrderService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputPurchaseOrderOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputPurchaseOrderOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }


    }
}
