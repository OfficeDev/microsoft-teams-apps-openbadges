// <copyright file="IssuerStaff.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing badge user information like user profile, role in Issuer group etc.
    /// </summary>
    public class IssuerStaff
    {
        /// <summary>
        /// Gets or sets profile of the badge user.
        /// </summary>
        [JsonProperty("userProfile")]
        public UserProfile UserProfile { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of badge user.
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets role of user in Issuer group.
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
