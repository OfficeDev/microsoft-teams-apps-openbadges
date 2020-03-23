// <copyright file="EarnedBadgeResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System;

    /// <summary>
    /// Class contains details of the earned badges by user.
    /// </summary>
    public class EarnedBadgeResponse
    {
        /// <summary>
        /// Gets or sets name of the badge.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the earned badge.
        /// </summary>
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets short description of the badge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the Issuer group who awarded the badge.
        /// </summary>
        public string AwardedBy { get; set; }

        /// <summary>
        /// Gets or sets the date on which the Issuer awarded the badge.
        /// </summary>
        public DateTime AwardedOn { get; set; }
    }
}
