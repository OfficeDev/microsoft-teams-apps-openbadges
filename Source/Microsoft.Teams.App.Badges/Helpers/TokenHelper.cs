// <copyright file="TokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Rest;
    using Microsoft.Teams.App.Badges.Models;

    /// <summary>
    /// Helper class for JWT token generation and validation.
    /// </summary>
    public class TokenHelper : ITokenHelper
    {
        /// <summary>
        /// Used to retrieve Badgr access token from Bot Framework.
        /// </summary>
        private readonly OAuthClient oAuthClient;

        /// <summary>
        /// Security key for generating and validating token.
        /// </summary>
        private readonly string securityKey;

        /// <summary>
        /// Application base URL.
        /// </summary>
        private readonly string appBaseUri;

        /// <summary>
        /// Generic OAuth 2 connection name.
        /// </summary>
        private readonly string connectionName;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHelper"/> class.
        /// </summary>
        /// <param name="botSettings">A set of key/value application configuration properties for Badges bot.</param>
        /// <param name="tokenSettings">A set of key/value application configuration properties for token.</param>
        /// <param name="oAuthSettings">A set of key/value application configuration properties for OAuth connection.</param>
        /// <param name="oAuthClient">Used to retrieve Badgr access token from Bot Framework.</param>
        /// <param name="logger">Sends logs to the Application Insights service.</param>
        public TokenHelper(
            IOptionsMonitor<BotSettings> botSettings,
            IOptionsMonitor<TokenSettings> tokenSettings,
            IOptionsMonitor<OAuthSettings> oAuthSettings,
            OAuthClient oAuthClient,
            ILogger<TokenHelper> logger)
        {
            this.securityKey = tokenSettings.CurrentValue.SecurityKey;
            this.appBaseUri = botSettings.CurrentValue.AppBaseUri;
            this.connectionName = oAuthSettings.CurrentValue.ConnectionName;
            this.oAuthClient = oAuthClient;
            this.logger = logger;
        }

        /// <summary>
        /// Generate JWT token used by client app to authenticate HTTP calls with API.
        /// </summary>
        /// <param name="serviceURL">Service URL from bot.</param>
        /// <param name="fromId">Unique Id from activity.</param>
        /// <param name="jwtExpiryMinutes">Expiry of token.</param>
        /// <returns>JWT token.</returns>
        public string GenerateInternalAPIToken(string serviceURL, string fromId, int jwtExpiryMinutes)
        {
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.securityKey));
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new List<Claim>()
                    {
                        new Claim("serviceURL", serviceURL),
                        new Claim("fromId", fromId),
                    }, "Custom"),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Issuer = this.appBaseUri,
                Audience = this.appBaseUri,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(securityTokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Get Badgr access token for user.
        /// </summary>
        /// <param name="fromId">Activity.From.Id from bot.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task<string> GetBadgrTokenAsync(string fromId)
        {
            try
            {
                var token = await this.oAuthClient.UserToken.GetTokenAsync(fromId, this.connectionName);
                return token?.Token;
            }
            catch (ValidationException ex)
            {
                this.logger.LogError(ex, "Properties passed to method are invalid or null.");
                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while retrieving user Badgr token");
                return null;
            }
        }
    }
}
