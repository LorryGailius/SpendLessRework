using SpendLess.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;

namespace SpendLess.Client.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly ISnackBarService _snackBarService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public event EventHandler<EventArgs>? FamilyChanged;

        public Family Family { get; set; } = new Family("-1");
        public int Permission { get; set; } = 1;
        public List<UserDto> Users { get; set; }
        public List<Transactions> FamilyTransactions { get; set; } = new List<Transactions>();

        public async Task CreateFamily(string name)
        {
            Family family = new Family();
            family.Name = name;
            family.Balance = 0;

            var client = _clientFactory.CreateClient();

            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                var response = await client.PostAsJsonAsync("https://localhost:7290/api/Family/CreateFamily", family);
                if (response.IsSuccessStatusCode)
                {
                    int id = await response.Content.ReadFromJsonAsync<int>();
                    Family.Name = name;
                    await this.OnFamilyChanged();
                    _snackBarService.SuccessMsg($"Family is {id}");
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }
        }

        public async Task GetFamily()
        {
            var client = _clientFactory.CreateClient();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Family/GetFamily");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {

                    if (response.Content != null)
                    {
                        var result = await response.Content.ReadFromJsonAsync<Family>();
                        Family = result;
                    }
                }

            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

            await this.OnFamilyChanged();
        }

        public async Task JoinFamily(int familyId)
        {
            var client = _clientFactory.CreateClient();

            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7290/api/Family/Join/{familyId}");

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    await this.OnFamilyChanged();
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

        }

        public async Task GetFamilyMembers()
        {
            var client = _clientFactory.CreateClient();

            if (Family.Name != "-1")
            {
                try
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Family/GetFamilyMembers");
                    string token = await _localStorage.GetItemAsStringAsync("token");
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                    var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {

                        if (response.Content != null)
                        {
                            var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                            Users = result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                    throw;
                }

                await this.OnFamilyChanged();
            }
        }

        public FamilyService(IHttpClientFactory clientFactory, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, ISnackBarService snackBarService)
        {
            _clientFactory = clientFactory;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _snackBarService = snackBarService;
        }

        public async Task OnFamilyChanged()
        {
            if (FamilyChanged is not null)
                FamilyChanged.Invoke(this, EventArgs.Empty);
        }

        public async Task GetFamilyTransactions()
        {
            var client = _clientFactory.CreateClient();
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Family/GetFamilyTransactions");
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
                    var result = await response.Content.ReadFromJsonAsync<List<Transactions>>();
                    FamilyTransactions = result;
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

            await this.OnFamilyChanged();
        }


        public async Task GetPermission()
        {
            var client = _clientFactory.CreateClient();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7290/api/Family/GetPermission");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {

                    if (response.Content != null)
                    {
                        var result = await response.Content.ReadFromJsonAsync<int>();
                        Permission = result;
                    }
                }

            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }

            await this.OnFamilyChanged();
        }

        public async Task ChangeUsername(int id, string newUsername)
        {
            var client = _clientFactory.CreateClient();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:7290/api/Family/ChangeUsername/{id}/{newUsername}");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var user = Users.FirstOrDefault(u => u.Id == id);
                    user.Username = newUsername;
                    await this.OnFamilyChanged();
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }
        }

        public async Task KickUser(int id)
        {
            var client = _clientFactory.CreateClient();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:7290/api/Family/Kick/{id}");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var user = Users.FirstOrDefault(u => u.Id == id);
                    Users.Remove(user);
                    await this.OnFamilyChanged();
                }
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync("https://localhost:7290/api/Exception", ex);
                throw;
            }
        }
    }
}
