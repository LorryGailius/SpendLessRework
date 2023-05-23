using SpendLess.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace SpendLess.Client.Services
{

    public class TransactionService : ITransactionService
    {

        public event EventHandler<EventArgs>? TransactionsChanged;
        public event EventHandler<EventArgs>? PrivelagesChanged;

        public async Task OnTransactionsChanged()
        {
            if (TransactionsChanged is not null)
                TransactionsChanged.Invoke(this, EventArgs.Empty);
        }

        public async Task OnPrivelagesChanged()
        {
            if (PrivelagesChanged is not null)
                PrivelagesChanged.Invoke(this, EventArgs.Empty);
        }

        public TransactionService(IHttpClientFactory clientFactory, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, ISnackBarService snackBarService)
        {
            _clientFactory = clientFactory;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _snackBarService= snackBarService;
        }

        private readonly ISnackBarService _snackBarService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        public List<SpendLess.Shared.Transactions> Transactions { get; set; } = new List<SpendLess.Shared.Transactions>();
        public string UserName { get; set; } = "Name not found";

        public bool IsAdmin { get; set; } = false;

        public int UserId { get; set; }

        public delegate void LogException(HttpClient client, string str, Exception ex);

        public async Task<bool> Savelist(double? amount, bool toggleExpenseIncome, string? textValue, string? categoryValue, DateTime? date, bool togglePeriodical, int interval, string period, DateTime? endDate)
        {
            if (amount < 0){
                //SnackBarService.WarningMsg("Amount can not be negative or zero!");
                return false;
            }
            if (toggleExpenseIncome == true)
            {
                categoryValue = "Income";
            }
            if (textValue == null){
                textValue = "Transaction";
            }
            if (categoryValue != null && date.HasValue && amount != null){
                if (toggleExpenseIncome == false){
                    amount = -amount;
                }

                if (togglePeriodical == true)
                {
                    await AddPeriodicTransaction(amount, categoryValue, date ?? DateTime.MinValue, textValue, period, interval, endDate);
                }
                else
                {
                    await AddTransaction(amount, categoryValue, date ?? DateTime.MinValue, textValue);
                }
                Transactions.Sort();
                await Task.Delay(1);

                categoryValue = null;
                amount = null;
                return true;
            }

            else{
                //SnackBarService.WarningMsg("Fields can not be empty!");
                return false;
            }
            textValue = null;
            await OnTransactionsChanged();
        }




        public async Task GetTransactions(Services.LogException logException)
        {           
            var client = _clientFactory.CreateClient();
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetTransactions");                
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                if ((response.StatusCode) == HttpStatusCode.Unauthorized){
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    return;
                }
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Transactions>>();
                    
                    Transactions = result;
                }              
            }
    //        catch (NullReferenceException ex)
    //        {
				//logException(client, $"{Constants.ApiUrl}/Exception", ex);
				//throw;
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            logException(client, $"{Constants.ApiUrl}/Exception", ex);
    //            throw;
    //        }
    //        catch (JsonException ex)
    //        {
    //            logException(client, $"{Constants.ApiUrl}/Exception", ex);
    //            throw;
    //        }
            catch (Exception ex){
                logException(client, $"{Constants.ApiUrl}/Exception", ex);
                throw;
            }


            /*var _httpClient = clientFactory.CreateClient();
            var result = await _httpClient.GetFromJsonAsync<List<Transaction>>("api/finance/GetTransactions");
            if(result != null)
            {
                Transactions = result;
                SnackBarService.SuccessMsg("Data loaded");
            }*/
            await this.OnTransactionsChanged();
        }

        public async Task AddTransaction(double? amount, string category, DateTime date, string comment)
        {
            var _httpClient = _clientFactory.CreateClient();
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");                
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));


                var transaction = new Transactions(null, amount, category, date, comment);
                var response = await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Transactions/AddTransaction", transaction);             
                if (response.IsSuccessStatusCode)
                {
                    var id = await response.Content.ReadFromJsonAsync<int>();
                    transaction.Id = id;
                    Transactions.Add(transaction);

                    _snackBarService.SuccessMsg("Succsesfully saved data");
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests){
                    _snackBarService.ErrorMsg("Slow down");
                    return;
                }
                else{
                    _snackBarService.ErrorMsg("Failed to save data!");
                }
            }
            //catch(NullReferenceException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (InvalidOperationException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch(JsonException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            catch(Exception ex){
                await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
        }

        public async Task AddFamilyTransaction(double amount, int recieverId)
        {
            var _httpClient = _clientFactory.CreateClient();
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                var userTransaction = new Transactions(null, amount, "Family", DateTime.Now.Date, "Transfer to family member");
                userTransaction.UserId = recieverId;
                Console.WriteLine(recieverId + " " + userTransaction.UserId);
                var response = await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Transactions/AddFamTransaction/{recieverId}", userTransaction);
                if (response.IsSuccessStatusCode)
                {
                    var saved = await response.Content.ReadFromJsonAsync<Transactions>();
                    Transactions.Add(saved);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _snackBarService.ErrorMsg("Slow down");
                    return;
                }
                else
                {
                    _snackBarService.ErrorMsg("Failed to save data!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }

            await this.OnTransactionsChanged();
        }

        public async Task AddPeriodicTransaction(double? amount, string category, DateTime date, string comment, string period, int interval, DateTime? endDate)
        {
            var _httpClient = _clientFactory.CreateClient();
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");               
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                if (endDate == null){
                    endDate = date.AddMonths(12);
                }
                bool isMonthly = false;
                switch (period) {
                    case "day(s)":
                        break;
                    case "week(s)":
                        interval = interval * 7;
                        break;
                    case "month(s)":
                        isMonthly = true;
                        break;
                    default: throw new ArgumentException("Period is incorrect"); //add exception for logging here?
                }

                List<Transactions> transactions = new List<Transactions>();

                while (date <= endDate)
                {
                    transactions.Add(new Transactions(null, amount, category, date, comment, null, period, interval, endDate));

                    if (isMonthly){
                        date = date.AddMonths(interval);
                    }
                    else{
                        date = date.AddDays(interval);
                    }
                }

                var response = await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Transactions/AddPeriodicTransaction", transactions);
                var transactionsID = await response.Content.ReadFromJsonAsync<List<Transactions?>>();

                if (response.IsSuccessStatusCode)
                {
                    Transactions.AddRange(transactionsID);
                    _snackBarService.SuccessMsg("Succsesfully saved data");
                }
                else{
                    _snackBarService.ErrorMsg("Failed to save data!");
                }
            }
            //catch (InvalidOperationException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch (JsonException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            //catch(ArgumentException ex)
            //{
            //    await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
            //    throw;
            //}
            catch (Exception ex){
                await _httpClient.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
        }

        public async Task<string> DeleteTransaction(int id)
        {
            try
            {
                string token = await _localStorage.GetItemAsStringAsync("token");
                var _httpClient = _clientFactory.CreateClient();
                _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                var response = await _httpClient.DeleteAsync($"{Constants.ApiUrl}/Transactions/{id}");
                if (response.IsSuccessStatusCode)
                {
                    _snackBarService.SuccessMsg("Transaction was successfully deleted");
                    
                    int c = 0;
                    foreach (var element in Transactions)
                    {
                        if (element.Id.Equals(id)){
                            Transactions.RemoveAt(c);
                            break;
                        }
                        c++;
                    }
                    return "Transaction was successfully deleted";
                }
                if (response.StatusCode == HttpStatusCode.TooManyRequests){

                    _snackBarService.ErrorMsg("Slow down");
                    return "Failed to delete transaction";
                }
                else{
                    _snackBarService.WarningMsg("Failed to delete transaction");
                    return "Failed to delete transaction";
                }
            }
            //catch (NullReferenceException ex)
            //{
            //    throw;
            //}
            //catch (InvalidOperationException ex)
            //{
            //    throw;
            //}
            //catch (JsonException ex)
            //{
            //    throw;
            //}
            catch (Exception ex){
                throw;
            }
        }

        public async Task GetTransactions()
        {
            var client = _clientFactory.CreateClient();
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetTransactions");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                if ((response.StatusCode) == HttpStatusCode.Unauthorized){
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    return;
                }
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<Transactions>>();
                    Transactions = result;
                }
            }
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
            catch (Exception ex) {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
            /*var _httpClient = clientFactory.CreateClient();
            var result = await _httpClient.GetFromJsonAsync<List<Transaction>>("api/finance/GetTransactions");
            if(result != null)
            {
                Transactions = result;
                SnackBarService.SuccessMsg("Data loaded");
            }*/
            await this.OnTransactionsChanged();
        }

        public async Task GetUserName()
        {
            var client = _clientFactory.CreateClient();
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetUser");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                if ((response.StatusCode) == HttpStatusCode.Unauthorized)
                {
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    UserName = "Name not found";
                }

                var result = await response.Content.ReadFromJsonAsync<Server.Models.User>();
                UserName = result.Name;
                UserId = result.Id;
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
        }

        public async Task GetIsAdmin()
        {
            var client = _clientFactory.CreateClient();
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Constants.ApiUrl}/Transactions/GetUser");
                string token = await _localStorage.GetItemAsStringAsync("token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Replace("\"", ""));

                var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                if ((response.StatusCode) == HttpStatusCode.Unauthorized)
                {
                    await _authStateProvider.GetAuthenticationStateAsync();
                    _snackBarService.ErrorMsg("Session has ended");
                    IsAdmin = false;
                }

                var result = await response.Content.ReadFromJsonAsync<Server.Models.User>();
                IsAdmin = (bool)result.IsAdmin;
                OnPrivelagesChanged();
            }
            catch (Exception ex)
            {
                await client.PostAsJsonAsync($"{Constants.ApiUrl}/Exception", ex);
                throw;
            }
        }
    }
}
