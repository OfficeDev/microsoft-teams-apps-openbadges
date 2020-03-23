// <copyright file="BadgeAccessToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains information about tokens of authenticated user at Badgr site.
    /// </summary>
    public class BadgeAccessToken
    {
        /// <summary>
        /// Gets or sets entity type, in this case “BadgeClass”.
        /// </summary>
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets unique identifier for the BadgeClass.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets details of an application.
        /// </summary>
        [JsonProperty("application")]
        public BadgrApplicationDetail Application { get; set; }

        /// <summary>
        /// Gets or sets scope of the entities Profile, Issuer and Backpack for eg: read/write or read.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets expiry date of the token.
        /// </summary>
        [JsonProperty("expires")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets token creation date.
        /// </summary>
        [JsonProperty("created")]
        public DateTime CreatedAt { get; set; }
    }
}
