//using Microsoft.AspNetCore.Mvc;
using SpendLess.Shared;
namespace SpendLess.Client.Services
{
    public delegate void LogException(HttpClient client, string str, Exception ex);

    public interface ITransactionService
    {
        public Task<bool> Savelist(double? amount, bool toggleExpenseIncome, string? textValue, string? categoryValue, DateTime? date, bool togglePeriodical, int interval, string period, DateTime? endDate);
        public List<Transactions> Transactions { get; set; }
        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        event EventHandler<EventArgs>? TransactionsChanged;

        event EventHandler<EventArgs>? PrivelagesChanged;
        public int UserId { get; set; }

        public Task OnTransactionsChanged();
        Task GetTransactions(LogException logexception);
        
        Task GetTransactions();

        Task AddTransaction(double? amount, string category, DateTime date, string comment = "Transaction");

        Task AddPeriodicTransaction(double? amount, string category, DateTime date, string comment, string period, int interval, DateTime? endDate);

        Task<string> DeleteTransaction(int id);

        Task AddFamilyTransaction(double amount, int recieverId);
        Task GetUserName();

        Task GetIsAdmin();
    }
}
