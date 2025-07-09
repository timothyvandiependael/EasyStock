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
    [Route("api/receptions")]
    public class ReceptionController : ControllerBase
    {
        private readonly IService<Reception> _service;
        private readonly IReceptionService _receptionService;
        private readonly IMapper _mapper;

        public ReceptionController(IService<Reception> service, IMapper mapper, IReceptionService receptionService)
        {
            _service = service;
            _mapper = mapper;
            _receptionService = receptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reception>>> GetAll()
        {
            var entities = await _receptionService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<OutputReceptionOverviewDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<Reception?>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<OutputReceptionDetailDto>(entity);
            dto.Supplier = _mapper.Map<OutputSupplierOverviewDto>(entity.Supplier);
            dto.Lines = _mapper.Map<List<OutputReceptionLineOverviewDto>>(entity.Lines);


            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] CreateReceptionDto dto)
        {
            if (dto == null) return BadRequest();
            var entity = _mapper.Map<Reception>(dto);
            await _receptionService.AddAsync(entity, HttpContext.User.Identity!.Name!);

            var resultDto = _mapper.Map<OutputReceptionDetailDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateReceptionDto dto)
        {
            if (dto == null || dto.Id != id) return BadRequest();
            var entity = _mapper.Map<Reception>(dto);
            await _service.UpdateAsync(entity, HttpContext.User.Identity!.Name!);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _receptionService.DeleteAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("block")]
        public async Task<ActionResult> Block(int id)
        {
            await _receptionService.BlockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("unblock")]
        public async Task<ActionResult> Unblock(int id)
        {
            await _receptionService.UnblockAsync(id, HttpContext.User.Identity!.Name!);
            return NoContent();
        }

        [HttpPost("advanced")]
        public async Task<ActionResult<PaginationResult<OutputReceptionOverviewDto>>> GetAdvanced([FromBody] AdvancedQueryParametersDto parameters)
        {
            if (parameters == null || parameters.Filters == null || parameters.Sorting == null) return BadRequest("Missing parameters");

            var result = await _receptionService.GetAdvancedAsync(parameters.Filters, parameters.Sorting, parameters.Pagination);
            var dtoItems = _mapper.Map<List<OutputReceptionOverviewDto>>(result.Data);

            return Ok(new PaginationResult<OutputReceptionOverviewDto>
            {
                Data = dtoItems,
                TotalCount = result.TotalCount
            });
        }
    }
}
