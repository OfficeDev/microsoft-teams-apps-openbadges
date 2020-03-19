// <copyright file="BadgeDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains details of badges earned in Badgr.
    /// </summary>
    public class BadgeDetail
    {
        /// <summary>
        /// Gets or sets name of the badge earned.
        /// </summary>
        [JsonProperty("name")]
        public BadgrResponseMetadata Name { get; set; }

        /// <summary>
        /// Gets or sets image URL of badge.
        /// </summary>
        [JsonProperty("image")]
        public BadgrResponseMetadata Image { get; set; }

        /// <summary>
        /// Gets or sets short description of the badge.
        /// </summary>
        [JsonProperty("description")]
        public BadgrResponseMetadata Description { get; set; }

        /// <summary>
        /// Gets or sets criteria for earning the badge.
        /// </summary>
        [JsonProperty("criteria_text")]
        public BadgrResponseMetadata CriteriaText { get; set; }

        /// <summary>
        /// Gets or sets the details of the Issuer group who awarded the badge.
        /// </summary>
        [JsonProperty("issuer")]
        public BadgeIssuer Issuer { get; set; }
    }
}
