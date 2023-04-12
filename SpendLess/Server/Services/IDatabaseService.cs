using SpendLess.Shared;

namespace SpendLess.Server.Services
{
    public interface IDatabaseService
    {
        Task AddNewUserAsync(User newUser);
        Task<byte[]?> GetUserPasswordHashAsync(UserDto request);
        Task<byte[]?> GetUserPasswordSaltAsync(UserDto request);
        Task<bool> FindEmail(string email);
        Task SaveChangesAsync();
        Task AddTransaction(Transactions transaction);
        Task<User> GetUser(string email);
        Task<List<Transactions>> GetTransactionsAsync(int userId);
        Task RemoveTransaction(Transactions transaction);
        Task<List<Ticket>> GetTicketAsync(int userId, bool userIsAdmin);
        Task<Ticket> GetTicketAsync(int id, int userId, bool userIsAdmin);
        Task AddTicket(Ticket ticket);
        Task RemoveTicket(int id);
        Task ResolveTicket(int id);
        Task<List<Message>> GetMessagesAsync(int id);
        Task AddMessage(String message, int id, int senderId);
        Task AddMessage(Message MessageObj);
    }
}