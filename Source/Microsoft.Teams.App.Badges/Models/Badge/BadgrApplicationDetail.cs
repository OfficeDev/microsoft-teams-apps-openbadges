// <copyright file="BadgrApplicationDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains details of the application which are connected to Badgr API.
    /// </summary>
    public class BadgrApplicationDetail
    {
        /// <summary>
        /// Gets or sets name of the application.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the application.
        /// </summary>
        [JsonProperty("image")]
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets website URL of the application.
        /// </summary>
        [JsonProperty("website_url")]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets ID of client which is connected to application.
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
    }
}
