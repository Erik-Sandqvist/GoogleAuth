using Microsoft.AspNetCore.Components.Authorization;
using NetcodeHub.Packages.Extensions.LocalStorage;
using Shared;
//using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Json;


namespace Blazor.State
{
    public class CustomAuthState(ILocalStorageService loalStoargeService) : AuthenticationStateProvider
    {
        private ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await loalStoargeService.GetItemAsStringAsync(Constant.Key);
                if(!string.IsNullOrEmpty(token))
                {
                    var tokenModel = JsonSerializer.Deserialize<Token>(token);
                    claimsPrincipal = SetClaimPrincipal(tokenModel.UserID);
                    return await Task.FromResult(new AuthenticationState(claimsPrincipal));
                }
                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch { return await Task.FromResult(new AuthenticationState(claimsPrincipal)); }
        }

        private ClaimsPrincipal SetClaimPrincipal(string userID)
        {
            try
            {
                Claim[] claims = [new(ClaimTypes.NameIdentifier, userID)];
                return new ClaimsPrincipal(new ClaimsIdentity(claims, Constant.Key));
            }
            catch { return new (new ClaimsIdentity()); }
        }

        public void NotifyAuthStateChanged() =>
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}
