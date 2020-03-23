// <copyright file="KeyVaultHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Helper to retrieve secrets from Azure key vault.
    /// </summary>
    [Serializable]
    public class KeyVaultHelper : IKeyVaultHelper
    {
        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Client class to perform cryptographic key operations and vault operations against the Azure Key Vault service.
        /// </summary>
        private readonly IKeyVaultClient keyVaultClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultHelper"/> class.
        /// Helper to retrieve secrets from Azure key vault..
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="keyVaultClient">Instance to perform cryptographic key operations and vault operations against the Azure Key Vault service.</param>
        public KeyVaultHelper(ILogger<KeyVaultHelper> logger, IKeyVaultClient keyVaultClient)
        {
            this.logger = logger;
            this.keyVaultClient = keyVaultClient;
        }

        /// <summary>
        /// Gets secret by Uri from Azure Key Vault.
        /// </summary>
        /// <param name="resourceUri">Uri of the secret from Azure Key Vault.</param>
        /// <returns> Returns secret value from Azure Key Vault. </returns>
        public async Task<string> GetSecretByUri(string resourceUri)
        {
            try
            {
                var secretData = await this.keyVaultClient.GetSecretAsync(resourceUri);
                if (secretData == null)
                {
                    this.logger.LogError("Key Vault does not have secret defined for given resourceUri.");
                    return null;
                }

                return secretData.Value;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetching secret from Azure Key Vault.");
                throw new Exception("Error while fetching secret from Azure Key Vault.");
            }
        }
    }
}
