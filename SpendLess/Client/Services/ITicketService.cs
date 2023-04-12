using SpendLess.Shared;

namespace SpendLess.Client.Services
{
    public interface ITicketService
    {
        public Task GetTickets();
        public Task<Ticket> GetTicket(int id);
        public List<Ticket> Tickets { get; set; }

        public Task ResolveTicket(Ticket ticket);

        public event EventHandler<EventArgs>? TicketsChanged;
    }
}
