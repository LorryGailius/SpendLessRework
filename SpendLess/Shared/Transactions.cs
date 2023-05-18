namespace SpendLess.Shared
{
    public class Transactions : IComparable<Transactions>
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public string? Comment { get; set; }
        public string Category { get; set; }

        private double amount;
        public double? Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                amount = Math.Round((value ?? 0), 2);
            }
        }
        public DateTime TransactionDate { get; set; }
        public string Period { get; set; }
        public int Interval { get; set; }
        public DateTime? EndDate { get; set; }

        public int? FamilyId { get; set; }

        public Transactions(int? Id, double? amount, string category, DateTime transactionDate, string comment = "Transaction", int? UserId = null,
                            String period = "oneTime", int interval = 0, DateTime? endDate = null)
        {
            this.Id = Id;
            this.Comment = comment;
            this.Amount = amount;
            this.Category = category;
            this.TransactionDate = transactionDate;
            this.Period = period;
            this.Interval = interval;
            this.EndDate = endDate;
        }

        public int CompareTo(Transactions x)
        {
            if (x.TransactionDate < this.TransactionDate)
            {
                return -1;
            }
            else if (x.TransactionDate > this.TransactionDate)
            {
                return 1;
            }
            else
                return 0;

            throw new NotImplementedException();
        }
    }


    public enum CategoryValues
    {
        Income, Housing, Transportation, Food, Utilities, Investing,
        Household, Personal, Gifts,
        Entertainment, Healthcare, Insurance, Kids,
        Pets, Subscriptions, Clothing, Travel, Technology, Family
    }
}