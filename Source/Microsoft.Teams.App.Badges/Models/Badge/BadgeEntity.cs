// <copyright file="BadgeEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing information about badges created in Issuer group.
    /// </summary>
    public class BadgeEntity
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
        /// Gets or sets URL of the OpenBadge compliant json.
        /// </summary>
        [JsonProperty("openBadgeId")]
        public string OpenBadgeURL { get; set; }

        /// <summary>
        ///  Gets or sets time-stamp when the BadgeClass was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets badgeUser who created this BadgeClass.
        /// </summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets entityId of the Issuer who owns the BadgeClass.
        /// </summary>
        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets URL of the OpenBadge Issuer compliant json.
        /// </summary>
        [JsonProperty("issuerOpenBadgeId")]
        public string IssuerOpenBadgeId { get; set; }

        /// <summary>
        /// Gets or sets name of the BadgeClass.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the BadgeClass.
        /// </summary>
        [JsonProperty("image")]
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets short description of the BadgeClass.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets external URL that describes in a human-readable format the criteria for the BadgeClass.
        /// </summary>
        [JsonProperty("criteriaUrl")]
        public string CriteriaUrl { get; set; }

        /// <summary>
        /// Gets or sets markdown formatted description of the criteria.
        /// </summary>
        [JsonProperty("criteriaNarrative")]
        public string CriteriaNarrative { get; set; }

        /// <summary>
        /// Gets or sets list of alignments which can optionally align to an educational standard.
        /// </summary>
        [JsonProperty("alignments")]
        public List<BadgeAlignment> Alignments { get; set; }

        /// <summary>
        /// Gets or sets List of tags that describe the BadgeClass.
        /// </summary>
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets badge expiration details. Badges will be marked as expired after expiration.
        /// </summary>
        [JsonProperty("expires")]
        public BadgeExpirationDetail Expiration { get; set; }
    }
}