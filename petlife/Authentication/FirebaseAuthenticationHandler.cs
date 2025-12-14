using System.Security.Claims;
using System.Text.Encodings.Web;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace petlife.Authentication;

public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FirebaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Authorization Header Not Found.");
        }

        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Authorization Scheme is not Bearer.");
        }

        var idToken = authorizationHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(idToken))
        {
            return AuthenticateResult.Fail("ID Token is missing.");
        }

        try
        {
            // Core verification via Firebase Admin SDK
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                new Claim("user_id", decodedToken.Uid)
            };

            // Email (optional)
            if (decodedToken.Claims.TryGetValue("email", out var emailObj))
            {
                var email = emailObj?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                }
            }

            // sign_in_provider (nested firebase claim)
            if (decodedToken.Claims.TryGetValue("firebase", out var firebaseObj) && firebaseObj is IDictionary<string, object> firebaseDict)
            {
                if (firebaseDict.TryGetValue("sign_in_provider", out var providerObj))
                {
                    var provider = providerObj?.ToString();
                    if (!string.IsNullOrEmpty(provider))
                    {
                        claims.Add(new Claim("firebase_provider", provider));
                    }
                }
            }

            // Include other custom claims except reserved ones already mapped
            foreach (var kv in decodedToken.Claims)
            {
                var key = kv.Key;
                if (key is "email" or "firebase" || key == "user_id") continue; // already processed
                var valueStr = kv.Value?.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    claims.Add(new Claim(key, valueStr));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (FirebaseAuthException ex)
        {
            Logger.LogError(ex, "Firebase ID Token verification failed: {Message}", ex.Message);
            return AuthenticateResult.Fail($"Token verification failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during Firebase token verification");
            return AuthenticateResult.Fail($"Unexpected error: {ex.Message}");
        }
    }
}
