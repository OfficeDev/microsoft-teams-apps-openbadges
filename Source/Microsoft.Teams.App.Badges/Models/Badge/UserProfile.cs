// <copyright file="UserProfile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class containing properties which have information about BadgeUser.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets type of the entity, in this case "BadgeUser".
        /// </summary>
        [JsonProperty("entityType")]
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets unique identifier for the BadgeUser.
        /// </summary>
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets first name of user.
        /// </summary>
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name of the user.
        /// </summary>
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets email IDs of the user.
        /// </summary>
        [JsonProperty("emails")]
        public List<BadgeUserEmail> Emails { get; set; }

        /// <summary>
        /// Gets or sets URL of user.
        /// </summary>
        [JsonProperty("url")]
        public List<string> Url { get; set; }

        /// <summary>
        /// Gets or sets telephone of user.
        /// </summary>
        [JsonProperty("telephone")]
        public List<string> Telephone { get; set; }

        /// <summary>
        /// Gets or sets badge API domain.
        /// </summary>
        [JsonProperty("badgrDomain")]
        public string BadgrDomain { get; set; }
    }
}
