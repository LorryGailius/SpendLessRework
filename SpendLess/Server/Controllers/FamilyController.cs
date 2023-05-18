using Microsoft.AspNetCore.Mvc;
using SpendLess.Server.Services;
using SpendLess.Shared;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpendLess.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly SpendLessContext _context;
        private readonly IFamilyService _service;

        public FamilyController(SpendLessContext context, IFamilyService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet("GetFamilyTransactions")]
        public async Task<ActionResult<List<Transactions>>> GetFamilyTransactions() =>
            await _service.GetTransactions(_context, HttpContext);

        [HttpGet("GetFamilyMembers")]
        public async Task<ActionResult<List<UserDto>>> GetFamilyMembers() =>
            await _service.GetFamilyMembers(_context, HttpContext);

        [HttpGet("GetFamily")]
        public async Task<ActionResult<Family>> GetFamily() =>
            await _service.GetFamily(_context, HttpContext);

        [HttpPost("ChangeUsername/{userId}/{name}")]
        public async Task ChangeUsername(int userId, string name) =>
            await _service.ChangeDisplayName(userId, name, _context, HttpContext);

        [HttpPost("CreateFamily")]
        public async Task<ActionResult<int?>> AddGroup([FromBody] Family? f) =>
            await _service.CreateFamily(f, _context, HttpContext);

        [HttpGet("Join/{id}")]
        public async Task<ActionResult<bool>> Join(int id) =>
            await _service.JoinFamily(id, _context, HttpContext);

        [HttpPost("ChangePermission/{id}/{permission}")]
        public async Task ChangePermission(int id, int permission) =>
            await _service.ChangePermissions(id, permission, _context, HttpContext);

        [HttpGet("GetPermission")]
        public async Task<ActionResult<int>> GetPermission() =>
            await _service.GetPermission(_context, HttpContext);

        [HttpPost("Kick/{id}")]
        public async Task Kick(int id) =>
            await _service.Kick(id, _context, HttpContext);
    }
}
