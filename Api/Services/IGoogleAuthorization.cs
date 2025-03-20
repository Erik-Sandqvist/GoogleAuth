using Api.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IGoogleAuthorization
    {
        string GetAuthorizatonUrl();

        Task<UserCredential> ExchangeCodeForToken(string code);

        Task<UserCredential> ValidateToken(string accessToken);
    }
    public class GoogleAuthorizationService(
        AppDbContext context, IGoogleAuthHelper googleHelper, IConfiguration config) : IGoogleAuthorization
    {
        private string RedirectUrl => config["Google:RedirectUri"]!;
        public async Task<UserCredential> ExchangeCodeForToken(string code)
        {
            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = googleHelper.GetClientSecrets(),
                    Scopes = googleHelper.GetScopes()
                });
            var token = await flow.ExchangeCodeForTokenAsync(
                "user", code, RedirectUrl, CancellationToken.None);
            context.Add(new Credential
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresInSeconds = token.ExpiresInSeconds?.ToString() ?? "0",
                IdToken = token.IdToken,
                UserId = Guid.NewGuid(),
                IssuedUtc = token.IssuedUtc,
            });
            await context.SaveChangesAsync();
            return new UserCredential(flow, "user", token);
        }



        public string GetAuthorizatonUrl() => 
            new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = googleHelper.GetClientSecrets(),
                    Scopes = googleHelper.GetScopes(),
                    Prompt = "consent"
                }).CreateAuthorizationCodeRequest(RedirectUrl).Build().ToString();
          
        

        public async Task<UserCredential> ValidateToken(string accessToken)
        {
            var _credential = await context.Credentials.FirstOrDefaultAsync(c => c.AccessToken == accessToken) ??
                throw new Exception("Token not found");
            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = googleHelper.GetClientSecrets(),
                    Scopes = googleHelper.GetScopes()
                });

            var tokenResponse = new TokenResponse
            {
                AccessToken = _credential.AccessToken,
                RefreshToken = _credential.RefreshToken,
                ExpiresInSeconds = long.TryParse(_credential.ExpiresInSeconds, out var expiresInSeconds) ? expiresInSeconds : (long?)null,
                IdToken = _credential.IdToken,
                IssuedUtc = _credential.IssuedUtc
            };
            return new UserCredential(flow, "user", tokenResponse);
        }
    }
}
