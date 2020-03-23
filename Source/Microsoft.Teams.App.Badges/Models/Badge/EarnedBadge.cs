// <copyright file="EarnedBadge.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Provides details about the badges earned by the recipient in backpack.
    /// </summary>
    public class EarnedBadge
    {
        /// <summary>
        /// Gets or sets details about badges earned by user from the json response received from API call.
        /// </summary>
        [JsonProperty("json")]
        public EarnedBadgeDetail EarnedBadgeDetail { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the earned badge.
        /// </summary>
        [JsonProperty("image")]
        public string ImageUri { get; set; }
    }
}
