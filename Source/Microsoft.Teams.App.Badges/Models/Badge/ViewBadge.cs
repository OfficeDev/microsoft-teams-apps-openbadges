// <copyright file="ViewBadge.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Class containing information of awarded badge.
    /// </summary>
    public class ViewBadge
    {
        /// <summary>
        /// Gets or sets name of user who awarded badge.
        /// </summary>
        public string AwardedBy { get; set; }

        /// <summary>
        /// Gets or sets name of badge to be awarded.
        /// </summary>
        public string BadgeName { get; set; }

        /// <summary>
        /// Gets or sets base64 encoded string of an image that represents the badge.
        /// </summary>
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets narrative that describes the achievement.
        /// </summary>
        public string Narrative { get; set; }

        /// <summary>
        /// Gets or sets list of users who received or will receive the award.
        /// </summary>
        public List<string> AwardRecipients { get; set; }

        /// <summary>
        /// Gets or sets from where task module is invoked.
        /// </summary>
        public string CommandContext { get; set; }
    }
}