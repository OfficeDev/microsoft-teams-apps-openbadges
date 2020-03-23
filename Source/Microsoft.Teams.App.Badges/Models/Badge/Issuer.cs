// <copyright file="Issuer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing details of Issuer group.
    /// </summary>
    public class Issuer
    {
        /// <summary>
        /// Gets or sets type of the entity, in this case "Issuer".
        /// </summary>
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets unique identifier for the Issuer.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets URL of the OpenBadge compliant json.
        /// </summary>
        [JsonProperty("openBadgeId")]
        public string OpenBadgeId { get; set; }

        /// <summary>
        /// Gets or Sets time-stamp when the Issuer was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or Sets badgeUser who created this Issuer.
        /// </summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets name of the Issuer.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the Issuer.
        /// </summary>
        [JsonProperty("image")]
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets the contact email for the Issuer.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets short description of the Issuer.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets homepage or website associated with the Issuer.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets badge user information like user profile, roles etc.
        /// </summary>
        [JsonProperty("staff")]
        public List<IssuerStaff> Staff { get; set; }
    }
}
