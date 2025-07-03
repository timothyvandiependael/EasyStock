using Microsoft.AspNetCore.Mvc;
using EasyStock.API.Models;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/StockMovements")]
    public class StockMovementController : ControllerBase
    {
        

    }
}