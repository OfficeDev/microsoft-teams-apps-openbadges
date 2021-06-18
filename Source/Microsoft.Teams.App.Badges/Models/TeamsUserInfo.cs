// <copyright file="TeamsUserInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    /// <summary>
    /// Claims which are added in JWT token.
    /// </summary>
    public class TeamsUserInfo
    {
        /// <summary>
        /// Gets or sets user Azure Active Directory object Id.
        /// </summary>
        public string AadObjectId { get; set; }

        /// <summary>
        /// Gets or sets channel id for the user or bot on this channel (Example: joe@smith.com, or @joesmith or 123456).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets email Id of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets display friendly name.
        /// </summary>
        public string Name { get; set; }
    }
}
