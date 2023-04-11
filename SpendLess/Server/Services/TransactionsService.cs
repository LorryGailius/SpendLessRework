using SpendLess.Shared;
using System.Security.Claims;

namespace SpendLess.Server.Services
{
    public class TransactionsService : ITransactionsService   
    {
        private readonly IDatabaseService _databaseService;
        public TransactionsService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Transactions>> GetTransactions(SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            var result = _databaseService.GetTransactionsAsync(user.Id);
            var transactions = result.Result;

            return transactions;
        }

        public async Task<int?> AddTransaction(Transactions? transaction, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            transaction.UserId = user.Id;

            await _databaseService.AddTransaction(transaction);
            await _databaseService.SaveChangesAsync();
            return transaction.Id;
        }

        public async Task<List<Transactions?>> AddPeriodicTransaction(List<Transactions> transactions, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            foreach (var transaction in transactions)
            {
                transaction.UserId = user.Id;
                _databaseService.AddTransaction(transaction);
            }
            await _databaseService.SaveChangesAsync();

            return transactions;
        }

        public async Task<bool> DeleteTransaction(int id, SpendLessContext _context)
        {
            if(id < 0)
            {
                return false;
            }

            var transaction = new Transactions(id, 0, "null", DateTime.MinValue);
            _databaseService.RemoveTransaction(transaction);
            await _databaseService.SaveChangesAsync();

            return true;
        }


        public async Task<User> GetUser(SpendLessContext _context, HttpContext _httpContext)
        {
            var identity = _httpContext.User.Identity as ClaimsIdentity;    
            var userClaims = identity.Claims;
            string email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
            var user = await _databaseService.GetUser(email);

            return user;
        }

        public async Task<List<Ticket>> GetTickets(SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);
            var result = _databaseService.GetTicketAsync(user.Id, user.IsAdmin);
            var tickets = result.Result;

            return tickets;
        }

        public async Task<Ticket> GetTicket(int id, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);
            var result = _databaseService.GetTicketAsync(id, user.Id, user.IsAdmin);
            var ticket = result.Result;

            return ticket;
        }

        public async Task<int?> AddTicket(Ticket? ticket, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = GetUser(_context, _httpContext);
            ticket.UserId = user.Id;

            _databaseService.AddTicket(ticket);
            _databaseService.SaveChangesAsync();

            return ticket.Id;
        }

        public async Task<bool> DeleteTicket(int id, SpendLessContext _context)
        {
            if (id < 0)
            {
                return false;
            }

            _databaseService.RemoveTicket(id);
            await _databaseService.SaveChangesAsync();
            return true;
        }

        public async Task<List<Message>> GetMessages(int id, SpendLessContext _context, HttpContext _httpContext)
        {
            var result = _databaseService.GetMessagesAsync(id);
            var messages = result.Result;
            return messages;
        }
    }
}
