using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/SalesOrderLines")]
    public class SalesOrderLineController : GenericController<SalesOrderLine>
    {
        public SalesOrderLineController(IService<SalesOrderLine> service) : base(service) { }

    }
}
