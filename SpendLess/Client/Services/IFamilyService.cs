using SpendLess.Shared;

namespace SpendLess.Client.Services
{
    public interface IFamilyService
    {
        public Task CreateFamily(String name);
        public Task GetFamily();
        public Family Family { get; set; }
        public List<UserDto> Users { get; set; }
        public List<Transactions> FamilyTransactions { get; set; }
        public int Permission { get; set; }
        public event EventHandler<EventArgs>? FamilyChanged;
        public Task JoinFamily(int familyId);
        public Task GetFamilyMembers();
        public Task GetFamilyTransactions();
        public Task OnFamilyChanged();
        public Task GetPermission();
        public Task ChangeUsername(int id, string newUsername);
        public Task KickUser(int id);
    }
}
