using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SpendLess.Server.Models;
using SpendLess.Shared;
using System.Runtime.CompilerServices;

namespace SpendLess.Server.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly SpendLessContext _context;


        public DatabaseService(SpendLessContext context)
        {
            _context = context;
        }

        public async Task<bool> FindEmail(string email) =>
            await _context.Users.AnyAsync(o => o.Email == email);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public async Task AddNewUserAsync(User newUser) =>
            await _context.Users.AddAsync(newUser);

        public async Task<byte[]?> GetUserPasswordHashAsync(UserDto request) =>
                 await _context.Users
                .Where(user => user.Email.ToLower().Contains(request!.Email!.ToLower()))
                .Select(user => user.PasswordHash)
                .FirstOrDefaultAsync();

        public async Task<byte[]?> GetUserPasswordSaltAsync(UserDto request) =>
         await _context.Users
        .Where(user => user.Email.ToLower().Contains(request!.Email!.ToLower()))
        .Select(user => user.PasswordSalt)
        .FirstOrDefaultAsync();

        public async Task AddTransaction(Transactions transaction) =>
            _context.Transactions.Add(transaction);

        public async Task<User> GetUser(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<Transactions>> GetTransactionsAsync(int userId) =>
            await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();

        public async Task RemoveTransaction(Transactions transaction){
            _context.Transactions.Attach(transaction);
            _context.Transactions.Remove(transaction);
        }

        public async Task<List<Ticket>> GetTicketAsync(int userId, bool userIsAdmin)
        {
            //Check if user is admin
            if (userIsAdmin)
            {
                Debug.WriteLine("User is admin");
                //Return tickets with status code 0
                return await _context.Tickets.Where(t => t.Status == 0 || (t.Status == 2 && t.UserId == userId)).ToListAsync();
            }

            System.Diagnostics.Debug.WriteLine("Not Admin:");
            //Return tickets of the user
            return await _context.Tickets.Where(t => t.UserId == userId).ToListAsync();
        }
        
    }
}
