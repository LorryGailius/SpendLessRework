using Newtonsoft.Json;
using SpendLess.Shared;
using System.Net.Http.Json;
using System.Text.RegularExpressions;


namespace SpendLess.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService(IHttpClientFactory clientFactory, ILocalStorageService LocalStorage, AuthenticationStateProvider authStateProvider, ISnackBarService snackBarService)
        {
            _clientFactory = clientFactory;
            _localStorage = LocalStorage;
            _authStateprovider = authStateProvider;
            _snackBarService = snackBarService;

        }

        private readonly ISnackBarService _snackBarService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly AuthenticationStateProvider _authStateprovider;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILocalStorageService _localStorage;
        private UserDto? UserInfo;


        public async Task<bool> ValidateLogin(bool success, string email, string username, string password)
        {

            if (success)
            {
                UserDto user = new UserDto
                {
                    Email = email,
                    Username = username,
                    Password = password
                };
                if (await GetLoginAuthentication(user))
                {
                    return true;

                }

            }
            return false;
        }

        public string? CheckPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password is required!";
            }
            if (password.Length < 8)
                return "Password must be at least of length 8";
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Password must contain at least one capital letter";
            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Password must contain at least one lowercase letter";
            if (!Regex.IsMatch(password, @"[0-9]"))
                return "Password must contain at least one digit";
            else
                return null;
        }


        public string? CheckEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "Email is required";

            }
            if (!Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                return "Email format is incorrect";
            else
                return null;

        }
        public async Task<bool> CreateAccount(UserDto user)
        {
            var client = _clientFactory.CreateClient();
            try
            {
                string serializedUser = JsonConvert.SerializeObject(user);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ApiUrl}/User/register");
                requestMessage.Content = new StringContent(serializedUser);
                requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync();
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result.token != null)
                    {
                        await _localStorage.SetItemAsync("token", result.token);
                        await _authStateprovider.GetAuthenticationStateAsync();
                        _snackBarService.SuccessMsg("Account has been created");
                        return true;
                    }
                    else{
                        _snackBarService.ErrorMsg(result.message);
                        return false;
                    }
                }
                else{
                    _snackBarService.ErrorMsg("Unexpected error");
                    return false;
                }
            }
            //catch (ArgumentNullException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (NullReferenceException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (InvalidOperationException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (JsonException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }



        }


        public async Task<bool> GetLoginAuthentication(UserDto user)
        {

            var client = _clientFactory.CreateClient();
            try
            {
                string serializedUser = JsonConvert.SerializeObject(user);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ApiUrl}/User/login");
                requestMessage.Content = new StringContent(serializedUser);
                requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result.token != null){
                        await _localStorage.SetItemAsync("token", result.token);
                        await _authStateprovider.GetAuthenticationStateAsync();
                        //await transactionService.GetTransactions();
                        //await BalanceService.RefreshBalance();
                        _snackBarService.SuccessMsg("Logged in");
                        return true;
                    }
                    else{
                        _snackBarService.ErrorMsg(result.message);
                        return false;
                    }
                }
                else {
                    _snackBarService.ErrorMsg("Server could not be reached.");
                    return false;
                }
            }
            //catch (ArgumentNullException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (NullReferenceException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (InvalidOperationException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (JsonException ex)
            //{
            //    await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }


        }

    }
}
