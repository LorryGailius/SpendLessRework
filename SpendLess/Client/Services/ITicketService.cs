using SpendLess.Shared;

namespace SpendLess.Client.Services
{
    public interface ITicketService
    {
        public Task GetTickets();
        public List<Ticket> Tickets { get; set; }

        public event EventHandler<EventArgs>? TicketsChanged;
    }
}
