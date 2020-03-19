// <copyright file="BadgeApiAppSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    /// <summary>
    /// Provides app settings related to Badge API.
    /// </summary>
    public class BadgeApiAppSettings
    {
        /// <summary>
        /// Gets or sets Badgr site rest api Uri.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets name of the entity in which user needs to be searched (in this case issuer group).
        /// </summary>
        public string IssuerEntityName { get; set; }
    }
}
