// <copyright file="BadgeExpirationDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains badge expiration details.
    /// </summary>
    public class BadgeExpirationDetail
    {
        /// <summary>
        /// Gets or sets duration amount this badge is generally valid for.
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the duration such as days, weeks, months and years.
        /// </summary>
        [JsonProperty("duration")]
        public string Duration { get; set; }
    }
}
