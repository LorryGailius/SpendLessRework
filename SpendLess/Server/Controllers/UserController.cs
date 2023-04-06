using Microsoft.AspNetCore.Mvc;
using SpendLess.Server.Services;
using SpendLess.Shared;

namespace SpendLess.Server.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IAuthServices _services;
        private readonly IConfiguration _configuration;

        public UserController(IAuthServices services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] UserDto request) =>
            await _services.Login(request, _configuration);
       
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] UserDto? request) =>
            await _services.Register(request, _configuration);

    }
}