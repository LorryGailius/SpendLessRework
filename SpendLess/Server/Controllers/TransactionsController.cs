using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Server.Middleware.Decorators;
using SpendLess.Shared;
using SpendLess.Server.Services;
using SpendLess.Server.Models;

namespace SpendLess.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly SpendLessContext _context;
        private readonly ITransactionsService _service;
        private readonly IFamilyService _familyService;

        public TransactionsController(SpendLessContext context, ITransactionsService service, IFamilyService familyService)
        {
            _context = context;
            _service = service;
            _familyService = familyService;
        }

        [HttpGet("GetTransactions")]
        public async Task<ActionResult<List<Transactions>>> GetTransactions() =>
            await _service.GetTransactions(_context, HttpContext);

        [HttpPost("AddTransaction")]
        [LimitRequests(MaxRequests = 1, TimeWindow = 1)]
        public async Task<ActionResult<int?>> AddTransaction([FromBody] Transactions? transaction) =>
            await _service.AddTransaction(transaction, _context, HttpContext);

        [HttpPost("AddPeriodicTransaction")]
        public async Task<ActionResult<List<Transactions?>>> AddPeriodicTransaction([FromBody] List<Transactions?> transactions) =>
            await _service.AddPeriodicTransaction(transactions, _context, HttpContext);

        [HttpDelete("{id}")]
        [LimitRequests(MaxRequests = 3, TimeWindow = 1)]
        public async Task DeleteTransaction(int id) =>
            await _service.DeleteTransaction(id, _context);

        [HttpGet("GetUser")]
        public async Task<ActionResult<User>> GetUser() =>
            await _service.GetUser(_context, HttpContext);

        [HttpGet("GetTickets")]
        public async Task<ActionResult<List<Ticket>>> GetTickets() =>
            await _service.GetTickets(_context, HttpContext);

        [HttpGet("GetTicket/{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id) =>
            await _service.GetTicket(id, _context, HttpContext);

        [HttpPost("AddTicket")]
        public async Task<ActionResult<int?>> AddTicket([FromBody] Ticket? ticket) =>
            await _service.AddTicket(ticket, _context, HttpContext);

        [HttpDelete("DeleteTicket/{id}")]
        public async Task DeleteTicket(int id) =>
            await _service.DeleteTicket(id, _context, HttpContext);

        [HttpDelete("ResolveTicket/{id}")]
        public async Task ResolveTicket(int id) =>
            await _service.ResolveTicket(id, _context, HttpContext);

        [HttpGet("GetFamilyTransactions")]
        public async Task<ActionResult<List<Transactions>>> GetFamilyTransactions() =>
            await _familyService.GetTransactions(_context, HttpContext);

        [HttpGet("GetFamily")]
        public async Task<ActionResult<List<User>>> GetFamilyGoals() =>
            await _familyService.GetFamilyMembers(_context, HttpContext);

        [HttpPost("ChangeUsername/{name}")]
        public async Task ChangeUsername(string name) =>
            await _familyService.ChangeDisplayName(name, _context, HttpContext);

        [HttpPost("AddGroup")]
        public async Task<ActionResult<int?>> AddGroup([FromBody] Family? f) =>
            await _familyService.CreateFamily(f, _context, HttpContext);

        [HttpPost("Join/{id}")]
        public async Task Join(int id) =>
            await _familyService.JoinFamily(id, _context, HttpContext);

        [HttpPost("ChangePermission/{id}/{permission}")]
        public async Task ChangePermission(int id, int permission) =>
            await _familyService.ChangePermissions(id, permission, _context, HttpContext);

    }
}
