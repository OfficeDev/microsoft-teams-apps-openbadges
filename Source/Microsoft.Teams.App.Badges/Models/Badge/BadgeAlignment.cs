// <copyright file="BadgeAlignment.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class contains alignment information which can optionally align to an educational standard.
    /// Alignment information may be relevant to people viewing an earner's awarded badges, or to a potential earner deciding whether to apply for the badge.
    /// </summary>
    public class BadgeAlignment
    {
        /// <summary>
        /// Gets or sets target name describing standard.
        /// </summary>
        [JsonProperty("targetName")]
        public string TargetName { get; set; }

        /// <summary>
        /// Gets or sets URL describing additional details about standard.
        /// </summary>
        [JsonProperty("targetUrl")]
        public string TargetUrl { get; set; }

        /// <summary>
        /// Gets or sets short description of standard.
        /// </summary>
        [JsonProperty("targetDescription")]
        public string TargetDescription { get; set; }

        /// <summary>
        /// Gets or sets framework of target.
        /// </summary>
        [JsonProperty("targetFramework")]
        public string TargetFramework { get; set; }

        /// <summary>
        /// Gets or sets code of target.
        /// </summary>
        [JsonProperty("targetCode")]
        public string TargetCode { get; set; }
    }
}
