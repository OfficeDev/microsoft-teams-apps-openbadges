// <copyright file="BadgeIssuer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing details of the Issuer group who issued the award to recipient.
    /// </summary>
    public class BadgeIssuer
    {
        /// <summary>
        /// Gets or sets unique identifier of Issuer group.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of Issuer group.
        /// </summary>
        [JsonProperty("name")]
        public BadgrResponseMetadata Name { get; set; }

        /// <summary>
        /// Gets or sets description of the Issuer group.
        /// </summary>
        [JsonProperty("description")]
        public BadgrResponseMetadata Description { get; set; }

        /// <summary>
        /// Gets or sets email Id associated with Issuer group.
        /// </summary>
        [JsonProperty("email")]
        public BadgrResponseMetadata Email { get; set; }
    }
}
