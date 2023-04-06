using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SpendLess.Server.Services;
namespace SpendLess.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ExceptionController : ControllerBase
    {
        private readonly IExceptionService _service;
        public ExceptionController(IExceptionService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task LogException([FromBody] Exception? ex) => 
            await _service.LogException(ex, HttpContext.User.Identity as ClaimsPrincipal);         
    }
}
