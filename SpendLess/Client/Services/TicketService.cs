using System.Net;
using System.Net.Http.Headers;
using SpendLess.Shared;
using System.Net.Http.Json;

namespace SpendLess.Client.Services
{
    public class TicketService : ITicketService
    {
        public event EventHandler<EventArgs>? TicketsChanged;

        private readonly ISnackBarService _snackBarService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public delegate void LogException(HttpClient client, string str, Exception ex);
        public List<Ticket> Tickets { get; set; }

        public async Task GetTickets()
        {
            var client = _clientFactory.CreateClient();
            
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Transactions/GetTickets");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);


                if ((response.StatusCode) == HttpStatusCode.Unauthorized)
                {
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Ticket>>();
                    
                    result = result.OrderByDescending(x => x.Status).ToList();

                    Tickets = result;
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

            await this.OnTicketsChanged();
        }

        public TicketService(IHttpClientFactory clientFactory, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, ISnackBarService snackBarService)
        {
            _clientFactory = clientFactory;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _snackBarService = snackBarService;
        }

        public async Task OnTicketsChanged()
        {
            if(TicketsChanged is not null)
                TicketsChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
