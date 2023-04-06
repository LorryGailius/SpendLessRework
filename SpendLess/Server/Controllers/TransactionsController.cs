

using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Autofac.Extras.DynamicProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendLess.Server.Interceptor;
using SpendLess.Server.Middleware.Decorators;
using SpendLess.Shared;
using SpendLess.Server.Services;
using System.Security.Claims;
using System.Text.Json;
using Autofac.Core;

namespace SpendLess.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly SpendLessContext _context;
        private readonly ITransactionsService _service;

        public TransactionsController(SpendLessContext context, ITransactionsService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet("GetTransactions")]
        public async Task<ActionResult<List<Transactions>>> GetTransactions() =>
            await _service.GetTransactions(_context, HttpContext);

        [HttpGet("GetTickets")]
        public async Task<ActionResult<List<Ticket>>> GetTickets() =>
            await _service.GetTickets(_context, HttpContext);

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

    }
}
