namespace SpendLess.Shared
{
    public class Ticket
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? SupportId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public int Status { get; set; }
    }
}
