

using Microsoft.AspNetCore.Mvc;
using SpendLess.Shared;

namespace SpendLess.Server.Services
{
    public interface IFamilyService
    {

        public Task<List<Transactions>> GetTransactions(SpendLessContext _context, HttpContext _httpContext);

        public Task<List<UserDto>> GetFamilyMembers(SpendLessContext _context, HttpContext _httpContext);
        public Task<Family> GetFamily(SpendLessContext _context, HttpContext _httpContext);
        public Task<int?> CreateFamily(Family family, SpendLessContext _context, HttpContext _httpContext);
        public Task ChangeDisplayName(int userId, String name, SpendLessContext _context, HttpContext _httpContext);
        public Task<bool> JoinFamily(int familyId, SpendLessContext _context, HttpContext _httpContext);
        public Task ChangePermissions(int userId, int permission, SpendLessContext context, HttpContext _httpContext);
        public Task<int> GetPermission(SpendLessContext context, HttpContext _httpContext);
        public Task Kick(int userId, SpendLessContext context, HttpContext _httpContext);
    }
}
