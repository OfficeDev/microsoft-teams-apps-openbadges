// <copyright file="SuperUserSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    /// <summary>
    /// Provides Badge API super user settings stored in Azure Key Vault.
    /// </summary>
    public class SuperUserSettings
    {
        /// <summary>
        /// Gets or sets Azure Key Vault base URL to retrieve secrets.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets secret name for super user name in Azure key vault.
        /// </summary>
        public string SuperUserNameKey { get; set; }

        /// <summary>
        /// Gets or sets secret name for super user password in Azure key vault.
        /// </summary>
        public string SuperUserPasswordKey { get; set; }
    }
}
