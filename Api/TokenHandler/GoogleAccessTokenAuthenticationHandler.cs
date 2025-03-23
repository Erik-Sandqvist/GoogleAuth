using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Api.TokenHandler
{
    public class GoogleAccessTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions>
                options, ILoggerFactory logger, UrlEncoder encoder, TimeProvider timeProvider,
            IGoogleAuthorization googleAuthorazation, AppDbContext context):
            AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private readonly TimeProvider timeProvider = timeProvider;

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            string authHeader = Request.Headers.Authorization!;
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid Authorization Header");

            string accessToken = authHeader["Bearer ".Length..].Trim();
            var userCredential = await googleAuthorazation.ValidateToken(accessToken);
            Credential? user = await GetUserCrededential(userCredential.Token.AccessToken);
            if (user == null)
                AuthenticateResult.Fail("InvalidAccess Token");

            List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, user!.UserId.ToString())];
            var identity = new ClaimsIdentity(claims, Constant.Scheme);
            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(identity), Constant.Scheme));
        }

        private async Task<Credential?> GetUserCrededential(string accessToken) =>
        await context.Credentials.FirstOrDefaultAsync(c => c.AccessToken == accessToken);
    }
    }

