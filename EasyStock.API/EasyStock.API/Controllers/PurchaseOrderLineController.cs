using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/PurchaseOrderLines")]
    public class PurchaseOrderLineController : GenericController<PurchaseOrderLine>
    {
        public PurchaseOrderLineController(IService<PurchaseOrderLine> service) : base(service) { }

    }
}