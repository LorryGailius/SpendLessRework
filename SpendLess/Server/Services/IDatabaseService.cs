﻿using SpendLess.Shared;

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
        Task<User> GetUserById(int id);
        Task<User> GetUser(string email);
        Task<List<Transactions>> GetTransactionsAsync(int userId);
        Task<List<Transactions>> GetTransactionsAsync(int userId, int familyId);
        Task RemoveTransaction(Transactions transaction);
        Task<List<Ticket>> GetTicketAsync(int userId, bool userIsAdmin);
        Task<Ticket> GetTicketAsync(int id, int userId, bool userIsAdmin);
        Task AddTicket(Ticket ticket);
        Task RemoveTicket(int id);
        Task ResolveTicket(int id);
        Task<List<Message>> GetMessagesAsync(int id);
        Task AddMessage(String message, int id, int senderId);
        Task AddMessage(Message MessageObj);
        Task<List<User>> GetFamilyMembers(int familyId, int userId);
        Task CreateGroup(Family family, int userId);
        Task ChangeDisplayName(int userId, string displayName);
        Task JoinFamily(int userId, int familyId);
        Task ChangePermissions(int userId, int permission);
        Task<String> GetFamilyName(int familyId);
        Task<Family> GetFamily(int familyId);
        Task RemoveUser(int userId);
    }
}