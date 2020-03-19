// <copyright file="IKeyVaultHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Helper to retrieve secrets from Azure key vault.
    /// </summary>
    public interface IKeyVaultHelper
    {
        /// <summary>
        /// Gets secret by Uri from Azure Key Vault.
        /// </summary>
        /// <param name="resourceUri"> Uri of the secret from Azure Key Vault.</param>
        /// <returns> Returns secret value from Azure Key Vault. </returns>
        Task<string> GetSecretByUri(string resourceUri);
    }
}