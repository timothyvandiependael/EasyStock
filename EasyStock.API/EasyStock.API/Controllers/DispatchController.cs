using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dispatches")]
    public class DispatchController : ControllerBase
    {
    }
}
