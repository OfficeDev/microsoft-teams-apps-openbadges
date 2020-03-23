// <copyright file="OwnerAccessToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains information about user token generated at Badgr site.
    /// </summary>
    public class OwnerAccessToken
    {
        /// <summary>
        /// Gets or sets access token of user.
        /// </summary>
        [JsonProperty("access_token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets type of access token.
        /// </summary>
        [JsonProperty("token_type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets expiry date of the token (in seconds).
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpirationDuration { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets scope of the entities like Profile, Issuer and Backpack for eg: read/write or read.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}
