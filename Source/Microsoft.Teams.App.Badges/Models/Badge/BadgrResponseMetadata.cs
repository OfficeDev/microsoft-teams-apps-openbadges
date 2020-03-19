// <copyright file="BadgrResponseMetadata.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class maps with json response received from API call.
    /// </summary>
    public class BadgrResponseMetadata
    {
        /// <summary>
        /// Gets or sets type of information stored in Value or Id field.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets value of the data.
        /// </summary>
        [JsonProperty("@value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets id of the data.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
