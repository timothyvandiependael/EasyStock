using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Suppliers")]
    public class SupplierController : GenericController<Supplier>
    {
        public SupplierController(IService<Supplier> service) : base(service) { }

    }
}