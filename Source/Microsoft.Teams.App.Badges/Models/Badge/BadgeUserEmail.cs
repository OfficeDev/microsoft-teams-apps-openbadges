// <copyright file="BadgeUserEmail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing properties which have email details of user.
    /// </summary>
    public class BadgeUserEmail
    {
        /// <summary>
        /// Gets or sets email address associated with a BadgeUser.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email address has been verified.
        /// </summary>
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether single email address to receive email notifications.
        /// </summary>
        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }
}
