using Autofac.Extras.DynamicProxy;
using Microsoft.IdentityModel.Tokens;
using SpendLess.Server.Interceptor;
using SpendLess.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
namespace SpendLess.Server.Services
{
    [Intercept(typeof(UnhandledExceptionLogger))]
    public class AuthServices : IAuthServices
    {
        private readonly IDatabaseService _databaseService;
        //private readonly IConfiguration _configuration;
        public AuthServices(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        public async Task<bool> CreateAccount(UserDto request)
        {
            if (VerifyRequest(request) && !await VerifyEmail(request.Email!))
            {
                byte[] passwordHash;
                byte[] passwordSalt;
                CreatePasswordHash(request.Password!, out passwordHash, out passwordSalt);
                User newUser = new User
                {
                    Email = request.Email!,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Name = request.Username,
                    InitialBalance = 0
                };
                await _databaseService.AddNewUserAsync(newUser);
                await  _databaseService.SaveChangesAsync();
                return true;

            }
            return false;
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> VerifyAccount(UserDto request)
        {

            if (VerifyRequest(request))
            {
                if (await VerifyEmail(request.Email!))
                {
                    var passwordHash = await _databaseService.GetUserPasswordHashAsync(request);
                    var passwordSalt = await _databaseService.GetUserPasswordSaltAsync(request);
                    await _databaseService.SaveChangesAsync();
                    if (passwordHash == null || passwordSalt == null)
                    {
                        return false;
                    }

                    using (var hmac = new HMACSHA512(passwordSalt!))
                    {
                        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));
                        return computedHash.SequenceEqual(passwordHash!);
                    }

                }
            }
            return false;

        }

        public async Task<LoginResponse> Register(UserDto request, IConfiguration _configuration)
        {
            if (VerifyRequest(request!))
            {
                Log.Information(request.Email!);
                if (await CreateAccount(request!))
                {
                    string? token = CreateToken(request!, _configuration);
                    return new LoginResponse(token, "Success");
                }
                return new LoginResponse(null, "User already exists");
            }
            return new LoginResponse(null, "Input not valid");
        }


        public async Task<LoginResponse> Login(UserDto request, IConfiguration _configuration)
        {
            if (VerifyRequest(request!))
            {
                Log.Information(request.Email!);
                if (!await VerifyAccount(request!)){
                    return new LoginResponse(null, "User with this email and password is not found");
                }
                else
                {
                    string token = CreateToken(request!, _configuration);
                    return new LoginResponse(token, "Success");
                }
            }
            else return new LoginResponse(null, "Input not valid");
        }


        public async Task<bool> VerifyEmail(string email)
        {
                var smth = await _databaseService.FindEmail(email);
                return smth;
        }

        public string? CreateToken(UserDto user, IConfiguration _configuration)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            return jwt;
        }

        public bool VerifyRequest(UserDto request)
        {
            if (request != null && request.Email != null && request.Password != null)
            {
                return true;
            }
            return false;
        }




        /*
        public RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        public void SetRefreshToken(RefreshToken newRefreshToken, HttpResponse Response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
        }
        */

    }
}