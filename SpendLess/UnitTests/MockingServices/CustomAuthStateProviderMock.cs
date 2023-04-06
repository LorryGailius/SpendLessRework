using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace UnitTests.MockingServices
{
    public class CustomAuthStateProviderMock : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
    }
}
