using SpendLess.Shared;
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

        public async Task CreateFamily(string name)
        {
            Family family = new Family();
            family.Name = name;

            var client = _clientFactory.CreateClient();

            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                var response = await client.PostAsJsonAsync("https://localhost:7290/api/Transactions/AddGroup", family);
                if(response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                    int id = await response.Content.ReadFromJsonAsync<int>();
                    Console.WriteLine($"Family id is {id}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public FamilyService(IHttpClientFactory clientFactory, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, ISnackBarService snackBarService)
        {
            _clientFactory = clientFactory;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _snackBarService = snackBarService;
        }
    }
}
