// <copyright file="AssertionDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains details of the assertion to be awarded and recipient of the award.
    /// </summary>
    public class AssertionDetail
    {
        /// <summary>
        /// Gets or sets unique identifier of Issuer group.
        /// </summary>
        [JsonProperty("issuer")]
        public string IssuerId { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of badge class.
        /// </summary>
        [JsonProperty("badge_class")]
        public string BadgeClassId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notifications need to be sent to the recipient of an award.
        /// </summary>
        [JsonProperty("create_notification")]
        public bool CreateNotification { get; set; }

        /// <summary>
        /// Gets or sets details of the assertions to be awarded to recipients.
        /// </summary>
        [JsonProperty("assertions")]
        public List<BadgeAssertion> Assertions { get; set; }
    }
}
