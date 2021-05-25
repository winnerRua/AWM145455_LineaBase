using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WSREGGWMM.Services;

namespace WSREGGWMM.Helpers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IPartnerService _partnerService;
        private readonly IConfiguration _config;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IPartnerService partnerService,
            IConfiguration config)
            : base(options, logger, encoder, clock)
        {
            _partnerService = partnerService;
            _config = config;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("unauthorized");

            string authHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader))
                return AuthenticateResult.Fail("unauthorized");

            if (!authHeader.StartsWith("bearer", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("unauthorized");

            var token = authHeader.Substring("bearer".Length).Trim();

            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.Fail("unauthorized");

            try
            {
                return validateToken(token);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }

            //Partners partner = null;
            //try
            //{
            //    var request = "";

            //    using (var reader = new StreamReader(Request.Body))
            //    {
            //        request = reader.ReadToEnd();
            //    }

            //    if (string.IsNullOrEmpty(request))
            //        return AuthenticateResult.Fail("");

            //    var jsonRequest = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            //    var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            //    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            //    var username = credentials[0];
            //    var password = credentials[1];

            //    partner = await _partnerService.Authenticate(_config, username, password, jsonRequest["agencyCode"]);
            //}
            //catch
            //{
            //    return AuthenticateResult.Fail("");
            //}

            //if (partner == null)
            //    return AuthenticateResult.Fail("");

            //var claims = new[] {
            //   new Claim(ClaimTypes.NameIdentifier, partner.PartnerId.ToString()),
            //    new Claim(ClaimTypes.Name, partner.Username),
            //};
            //var identity = new ClaimsIdentity(claims, Scheme.Name);
            //var principal = new ClaimsPrincipal(identity);
            //var ticket = new AuthenticationTicket(principal, Scheme.Name);

            //return AuthenticateResult.Success(ticket);
        }

        private AuthenticateResult validateToken(string token)
        {
            var validatedToken = _partnerService.Tokens.FirstOrDefault(t => t.Key == token);

            if (validatedToken.Key == null)
                return AuthenticateResult.Fail("unauthorized");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, validatedToken.Value)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
