﻿using System.Net;
using System.Net.Http.Headers;
using SpendLess.Shared;
using System.Net.Http.Json;
using System.Net.Http;

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

        public async Task<Ticket> GetTicket(int id)
        {
            var client = _clientFactory.CreateClient();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Transactions/GetTicket/" + id);
                string token = await _localStorage.GetItemAsStringAsync("token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if ((response.StatusCode) == HttpStatusCode.Unauthorized)
                {
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Ticket>();
                    await this.OnTicketsChanged();

                    return result;
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

            return null;
        }

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

        public async Task ResolveTicket(Ticket ticket)
        {
            var client = _clientFactory.CreateClient();
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");

                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                var response = await client.DeleteAsync($"https://localhost:7290/api/Transactions/ResolveTicket/{ticket.Id}");

                if (response.IsSuccessStatusCode)
                {
                    _snackBarService.SuccessMsg("Transaction was successfully deleted");
                    Tickets.Remove(ticket);
                    await this.OnTicketsChanged();
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }
            Console.WriteLine("Updating tickets");
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
            if (TicketsChanged is not null)
                TicketsChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
