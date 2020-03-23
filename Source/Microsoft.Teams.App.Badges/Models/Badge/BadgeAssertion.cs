// <copyright file="BadgeAssertion.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains details of the assertion to be awarded to user.
    /// </summary>
    public class BadgeAssertion
    {
        /// <summary>
        /// Gets or sets email Id of the recipient of the award.
        /// </summary>
        [JsonProperty("recipient_identifier")]
        public string EmailId { get; set; }

        /// <summary>
        /// Gets or sets text which describes why recipient is awarded.
        /// </summary>
        [JsonProperty("narrative")]
        public string Narrative { get; set; }
    }
}
