using System.Security.Claims;

namespace SpendLess.Server.Services
{
    public class ExceptionService : IExceptionService
    {
        public async Task LogException(Exception? ex, ClaimsPrincipal identity)
        {
            if (ex != null)
            {
                try
                {
                    var userClaims = identity.Claims;
                    string email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email).Value;
                    var expDate = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Expiration).Value;
                    if (expDate != null && email != null){
                         Log.Error($"CLIENT SIDE ERROR\nUser's email: {email}\nToken exp.: {expDate}\nMessage:{ex.Message}\nStack trace: {ex.StackTrace}");
                    }
                    else
                    {
                         Log.Error($"CLIENT SIDE ERROR\nAnonymous user.\nMessage:{ex.Message}\nStack trace: {ex.StackTrace}");
                    }
                }
                catch (Exception ex1)
                {
                    Log.Error($"\nException message: {ex1.Message}\nException stack trace: {ex1.StackTrace}");
                    throw;
                }

            }

        }
    }
}
