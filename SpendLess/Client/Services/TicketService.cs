﻿using System.Net;
using System.Net.Http.Headers;
using SpendLess.Shared;
using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;

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
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetTicket/" + id);
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
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
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
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetTickets");
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


                    //put tickets with status 2 at the top of the list then 
                    result = result.OrderBy(ticket => ticket.Status == 2 ? 0 : ticket.Status == 0 ? 1 : 4).ToList();

                    Tickets = result;
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
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

                var response = await client.DeleteAsync($"{Constants.ApiUrl}/Transactions/ResolveTicket/{ticket.Id}");

                if (response.IsSuccessStatusCode)
                {
                    _snackBarService.SuccessMsg("Transaction was successfully resolved");
                    Tickets.Remove(ticket);

                    // Send resolve signal to support hub
                    HubConnection hubConnection = new HubConnectionBuilder()
                                                        .WithUrl($"{Constants.ApiUrl.Substring(0, Constants.ApiUrl.LastIndexOf("/api"))}/supporthub")
                                                        .Build();

                    await hubConnection.StartAsync();
                    await hubConnection.SendAsync("Resolve", ticket.Id);
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
            await this.OnTicketsChanged();
        }

        public async Task CreateTicket(Ticket ticket)
        {
            var client = _clientFactory.CreateClient();
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                var response = await client.PostAsJsonAsync($"{Constants.ApiUrl}/Transactions/AddTicket", ticket);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Succresss");
                    int id = await response.Content.ReadFromJsonAsync<int>();
                    ticket.Id = id;
                    Tickets.Add(ticket);
                    Tickets = Tickets.OrderBy(ticket => ticket.Status == 2 ? 0 : ticket.Status == 0 ? 1 : 4).ToList();
                    _snackBarService.SuccessMsg("Ticket was successfully submitted");
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
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
            if (TicketsChanged is not null)
                TicketsChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
