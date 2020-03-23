// <copyright file="TokenSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    /// <summary>
    /// Provides app setting related to jwt token.
    /// </summary>
    public class TokenSettings : OAuthSettings
    {
        /// <summary>
        /// Gets or sets random key to create jwt security key.
        /// </summary>
        public string SecurityKey { get; set; }
    }
}
