// <copyright file="IssuerHttpHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers.DelegatingHandlers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.App.Badges.Helpers;
    using Microsoft.Teams.App.Badges.Models;

    /// <summary>
    /// Delegating handler to get user Badgr access token and add it to HTTP request header.
    /// </summary>
    public class IssuerHttpHandler : DelegatingHandler
    {
        /// <summary>
        /// Represents a set of key/value super user configuration for Badges bot.
        /// </summary>
        private readonly SuperUserSettings superUserSettings;

        /// <summary>
        /// Microsoft Azure Key Vault base Uri.
        /// </summary>
        private readonly string keyVaultBaseUrl;

        /// <summary>
        /// Secret name for super user name from Microsoft Azure Key Vault.
        /// </summary>
        private readonly string superUserNameKey;

        /// <summary>
        /// Secret name for super user password from Microsoft Azure Key Vault.
        /// </summary>
        private readonly string superUserPasswordKey;

        /// <summary>
        /// Instance of key vault helper to retrieve key vault secrets.
        /// </summary>
        private IKeyVaultHelper keyVaultHelper;

        /// <summary>
        /// Helper to handle errors and get user details.
        /// </summary>
        private IBadgrApiHelper badgrApiHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuerHttpHandler"/> class.
        /// </summary>
        /// <param name="keyVaultHelper">Instance of key vault helper to retrieve secrets from Microsoft Azure Key Vault.</param>
        /// <param name="superUserSettings">A set of key/value super user configuration for Badges app.</param>
        /// <param name="badgrApiHelper">Instance of badge API helper.</param>
        public IssuerHttpHandler(IKeyVaultHelper keyVaultHelper, IOptionsMonitor<SuperUserSettings> superUserSettings, IBadgrApiHelper badgrApiHelper)
        {
            this.keyVaultHelper = keyVaultHelper;
            this.superUserSettings = superUserSettings.CurrentValue;
            this.keyVaultBaseUrl = this.superUserSettings.BaseUrl;
            this.superUserNameKey = this.superUserSettings.SuperUserNameKey;
            this.superUserPasswordKey = this.superUserSettings.SuperUserPasswordKey;
            this.badgrApiHelper = badgrApiHelper;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Get user Badgr access token from Microsoft Bot Framework.
            var ownerToken = await this.GetOwnerTokenFromBadgrAsync();

            // Add Badgr access token in header for Badgr API calls.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Gets owner token generated at Badgr.
        /// </summary>
        /// <returns>Returns owner token generated at Badgr API.</returns>
        private async Task<string> GetOwnerTokenFromBadgrAsync()
        {
            // Get owner credentials from Azure key Vault to check role of user and add user in Issuer group if needed.
            var superUserName = await this.keyVaultHelper.GetSecretByUri($"{this.keyVaultBaseUrl}/{this.superUserNameKey}");
            var superUserPassword = await this.keyVaultHelper.GetSecretByUri($"{this.keyVaultBaseUrl}/{this.superUserPasswordKey}");

            if (string.IsNullOrEmpty(superUserName) || string.IsNullOrEmpty(superUserPassword))
            {
                throw new Exception("Key Vault does not have secret defined for given resourceUri.");
            }

            // Get owner token from Badge API.
            return await this.badgrApiHelper.GetOwnerAccessTokenAsync(superUserName, superUserPassword);
        }
    }
}
