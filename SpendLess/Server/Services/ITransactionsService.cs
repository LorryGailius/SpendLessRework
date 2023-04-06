using SpendLess.Shared;
using System.Security.Claims;

namespace SpendLess.Server.Services
{
    public interface ITransactionsService
    {
        Task<List<Transactions>> GetTransactions(SpendLessContext _context, HttpContext _httpContext);
        Task<int?> AddTransaction(Transactions? transaction, SpendLessContext _context, HttpContext _httpContext);
        Task<List<Transactions?>> AddPeriodicTransaction(List<Transactions> transactions, SpendLessContext _context, HttpContext _httpContext);
        Task<bool> DeleteTransaction(int id, SpendLessContext _context);
        Task<User> GetUser(SpendLessContext _context, HttpContext _httpContext);
        Task<List<Ticket>> GetTickets(SpendLessContext _context, HttpContext _httpContext);
    }
}
