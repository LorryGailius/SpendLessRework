
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SpendLess.Server.Services;
using SpendLess.Shared;

public class SupportHub : Hub
{
    private readonly IDatabaseService _databaseService;
    public SupportHub(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(int user, int ticketId, string message)
    {
        Message tempMessage = new Message();
        tempMessage.ticketID = ticketId;
        tempMessage.senderID = user;
        tempMessage.message = message;
        tempMessage.date = DateTime.Now;

        await _databaseService.AddMessage(tempMessage);

        await Clients.Group(ticketId.ToString()).SendAsync("GetMessage", tempMessage);
        //await Clients.All.SendAsync("GetMessage", tempMessage);
    }

    public async Task JoinGroup(int ticketId)
    {
        List<Message> history = await _databaseService.GetMessagesAsync(ticketId);
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId.ToString());

        foreach(var message in history)
        {
            await Clients.Group(ticketId.ToString()).SendAsync("GetMessage", message);
        }
    }

}