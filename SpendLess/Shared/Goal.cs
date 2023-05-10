namespace SpendLess.Shared
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
        public double? Amount { get; set; }
        public DateTime EndDate { get; set; }
        public double? CurrentAmount { get; set; }
        public int? FamilyId { get; set; }

        public Goal(int id, int userId, string name, double? amount, DateTime endDate, double? currentAmount) 
        {
            this.Id = id;
            this.UserId = userId;
            this.Name = name;
            this.Amount = amount;
            this.EndDate= endDate;
            this.CurrentAmount = currentAmount;
            this.FamilyId = FamilyId;
        }
    }
}
