using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendLess.Shared;

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

        // Get all tickets
        public async Task<List<Ticket>> GetTicketAsync(int userId, bool userIsAdmin)
        {
            //Check if user is admin
            if (userIsAdmin)
            {
                //Return tickets with status code 0 or in progress my specific admin
                return await _context.Tickets.Where(t => t.Status == 0 || (t.Status == 2 && t.SupportId == userId)).ToListAsync();
            }
            //Return tickets of the user
            return await _context.Tickets.Where(t => t.UserId == userId).ToListAsync();
        }

        // Get ticket by id
        public async Task<Ticket> GetTicketAsync(int id, int userId, bool userIsAdmin)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (userIsAdmin || ticket.UserId == userId)
            {
                return ticket;

            }
            
            // return forbidden
            throw new Exception("Forbidden");
        }

        public async Task AddTicket(Ticket ticket)
        {
            // Add ticket to database
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTicket(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);

            //Remove all messages from ticket
            var messages = _context.Messages.Where(m => m.ticketID == id);
            _context.Messages.RemoveRange(messages);

            _context.Tickets.Remove(ticket);
            await SaveChangesAsync();
        }
        
        public async Task ResolveTicket(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);
            ticket.Status = 1;
            _context.Tickets.Update(ticket);
            await SaveChangesAsync();
        }

        public async Task<List<Message>> GetMessagesAsync(int id)
        {
            return await _context.Messages.Where(m => m.ticketID == id).ToListAsync();
        }

        public async Task AddMessage(String message, int id, int senderId)
        {
            Message temp = new Message();
            temp.ticketID = id;
            temp.senderID = senderId;
            await _context.Messages.AddAsync(temp);
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);
            // Change description to message
            ticket.Description = temp.message;
            _context.Tickets.Update(ticket);
            await SaveChangesAsync();
        }   

        public async Task AddMessage(Message MessageObj)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == MessageObj.ticketID);

            if (ticket.Status != 1)
            {
                await _context.Messages.AddAsync(new Message { ticketID = MessageObj.ticketID, message = MessageObj.message, date = MessageObj.date, senderID = MessageObj.senderID });

                //Check if sender is admin
                var sender = _context.Users.FirstOrDefault(u => u.Id == MessageObj.senderID);

                if (sender.IsAdmin)
                {
                    //Change ticket status to in progress
                    ticket.Status = 2;
                    ticket.SupportId = sender.Id;
                }
                else
                {
                    //Change ticket status to open
                    ticket.Status = 0;
                }

                ticket.Description = MessageObj.message;

                _context.Tickets.Update(ticket);

                await SaveChangesAsync();
            }
        }

        public async Task<List<Transactions>> GetTransactionsAsync(int userId, int familyId)
        {
            return await _context.Transactions.Where(t => t.FamilyId == familyId).ToListAsync();
        }

        public async Task<List<User>> GetFamilyMembers(int familyId, int userId)
        {
            return await _context.Users.Where(u => u.FamilyId == familyId && u.Id != userId).ToListAsync();
        }

        public async Task<String> GetFamilyName(int familyId)
        {
            return await _context.Families.Where(f => f.Id == familyId).Select(f => f.Name).FirstOrDefaultAsync();
        }

        public async Task CreateGroup(Family family, int userId)
        {
            family.Balance = 0;
            await _context.Families.AddAsync(family);
            await SaveChangesAsync();
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.Permission = 2);
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.FamilyId = family.Id);
            await SaveChangesAsync();
        }

        public async Task ChangeDisplayName(int userId, string displayName)
        {
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.Username = displayName);
            await SaveChangesAsync();
        }

        public async Task JoinFamily(int userId, int familyId)
        {
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.FamilyId = familyId);
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.Permission = 1);
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.Username = u.Name);
            await SaveChangesAsync();
        }

        public async Task ChangePermissions(int userId, int permission)
        {
            await _context.Users.Where(u => u.Id == userId).ForEachAsync(u => u.Permission = permission);
            await SaveChangesAsync();
        }

        public async Task<Family> GetFamily(int familyId)
        {
            return await _context.Families.FirstOrDefaultAsync(f => f.Id == familyId);
        }
    }
}
