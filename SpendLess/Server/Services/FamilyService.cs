using SpendLess.Shared;
using System.Net.Sockets;
using System.Security.Claims;

namespace SpendLess.Server.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IDatabaseService _databaseService;
        public FamilyService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task ChangeDisplayName(String name, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            if (user == null) { return; }

            await _databaseService.ChangeDisplayName(user.Id, name);
        }

        public async Task ChangePermissions(int userId, int permission, SpendLessContext _context, HttpContext _httpContext)
        {
            await _databaseService.ChangePermissions(userId, permission);
        }

        public async Task<int?> CreateFamily(Family family, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            await _databaseService.CreateGroup(family, user.Id);
            return family.Id;
        }

        public async Task<List<User>> GetFamilyMembers(SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            List<User> familyMembers = new List<User>();

            if (user.FamilyId != null)
            {
                familyMembers = await _databaseService.GetFamilyMembers(user.Id);
            }

            return familyMembers;
        }

        public async Task<List<Transactions>> GetTransactions(SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            List<Transactions> transactions = new List<Transactions>();

            if (user.FamilyId != null)
            {
                transactions = await _databaseService.GetTransactionsAsync(user.Id, (int)user.FamilyId);
            }


            return transactions;

        }

        public async Task JoinFamily(int familyId, SpendLessContext _context, HttpContext _httpContext)
        {
            var user = await GetUser(_context, _httpContext);

            if (user != null)
            {
                await _databaseService.JoinFamily(user.Id, familyId);
            }
        }


        public async Task<User> GetUser(SpendLessContext _context, HttpContext _httpContext)
        {
            var identity = _httpContext.User.Identity as ClaimsIdentity;
            var userClaims = identity.Claims;
            string? email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
            var user = await _databaseService.GetUser(email);

            return user;
        }

    }
}
