
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SpendLess.Server.Services;

public class SupportHub : Hub
{
    DatabaseService _databaseService;

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}