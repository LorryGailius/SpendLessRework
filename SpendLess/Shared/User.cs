namespace SpendLess.Server.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }/* = Encoding.ASCII.GetBytes("Kazys123");*/
        public byte[] PasswordSalt { get; set; }/* = Encoding.ASCII.GetBytes("Kazys123");*/
        public string? Name { get; set; } = null;
        public bool IsAdmin { get; set; } = false;
        public int? FamilyId { get; set; } = null;
        public int? Permission { get; set; } = null;
        public string? Username { get; set; } = null;
        public int? InitialBalance { get; set; }

    }
}