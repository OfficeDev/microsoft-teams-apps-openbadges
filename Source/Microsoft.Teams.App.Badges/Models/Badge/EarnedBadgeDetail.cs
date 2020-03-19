// <copyright file="EarnedBadgeDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing details about badges earned by user.
    /// </summary>
    public class EarnedBadgeDetail
    {
        /// <summary>
        /// Gets or sets badge details from json received in API call response.
        /// </summary>
        [JsonProperty("badge")]
        public BadgeDetail BadgeDetail { get; set; }

        /// <summary>
        /// Gets or sets the date on which the badge was awarded from json received in API call response.
        /// </summary>
        [JsonProperty("issuedOn")]
        public BadgrResponseMetadata IssuedOn { get; set; }
    }
}
