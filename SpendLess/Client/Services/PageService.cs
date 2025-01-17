﻿namespace SpendLess.Client.Services
{
    public class PageService : IPageService
    {
        private readonly ITransactionService _transactionService;
        private readonly IFamilyService _familyService;

        public PageService(ITransactionService transactionService, IFamilyService familyService)
        {
            _transactionService = transactionService;
            _familyService = familyService;
        }

        public async Task<IEnumerable<string>> Search(string value)
        {
            await Task.Delay(5);

            string[] categoryVal = Enum.GetNames(typeof(SpendLess.Shared.CategoryValues));


            // if text is null or empty, show complete list
            if (string.IsNullOrEmpty(value))
                return categoryVal.Where(x => x != "Income");
            return categoryVal.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase) && x != "Income");
        }

        public async Task DeleteRow(int id)
        {
            await _transactionService.DeleteTransaction(id);
            _transactionService.OnTransactionsChanged();
            _familyService.FamilyTransactions.RemoveAll(x => x.Id == id);
            _familyService.OnFamilyChanged();
        }

        public List<DateTime> GetDates(DateTime date)
        {
            int year = int.Parse(date.ToString("yyyy"));
            int month = int.Parse(date.ToString("MM"));

            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))
            .Select(day => new DateTime(year, month, day))
            .ToList();
        }
    }
}
