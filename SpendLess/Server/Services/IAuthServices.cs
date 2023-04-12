using SpendLess.Shared;
namespace SpendLess.Server.Services
{
    public interface IAuthServices
    {
        Task<bool> CreateAccount(UserDto request);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        string? CreateToken(UserDto user, IConfiguration _configuration);
        Task<bool> VerifyAccount(UserDto request);
        bool VerifyRequest(UserDto request);
        Task<LoginResponse> Login(UserDto request, IConfiguration _configuration);
        Task<LoginResponse> Register(UserDto request, IConfiguration _configuration);
        Task<bool> VerifyEmail(string email);
    }
}