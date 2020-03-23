// <copyright file="ResourceController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Controllers
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.App.Badges.Resources;

    /// <summary>
    /// Controller to handle resource strings related request.
    /// </summary>
    [Route("api/resource")]
    [ApiController]
    public class ResourceController : ControllerBase
    {
        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceController"/> class.
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        public ResourceController(ILogger<ResourceController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Get resource strings for displaying in client app.
        /// </summary>
        /// <returns>Resource strings according to user locale.</returns>
        [Route("resourcestrings")]
        public IActionResult GetResourceStrings()
        {
            try
            {
                var strings = new
                {
                    Strings.SelectBadge,
                    Strings.CreateBadgeName,
                    Strings.CreateBadgeDescription,
                    Strings.EmptyAllBadgesTitle,
                    Strings.EmptyAllBadgesDescription,
                    Strings.InvalidTenant,
                    Strings.ExceptionResponse,
                    Strings.SessionExpired,
                    Strings.Badge,
                    Strings.BadgeCriteria,
                    Strings.BadgeDescription,
                    Strings.BadgeName,
                    Strings.AwardedBy,
                    Strings.OnDate,
                    Strings.EmptyYourBadgesTitle,
                    Strings.EmptyYourBadgesDescription,
                    Strings.AllBadges,
                    Strings.YourBadges,
                    Strings.SelectAtleastOneMember,
                    BadgeToAward = Strings.Badge,
                    Strings.ToBeAwardedTo,
                    Strings.SearchTeamMembers,
                    Strings.NoMatchesFound,
                    Strings.NoteForRecipients,
                    Strings.NoteForReceipientsPlaceholder,
                    Strings.Preview,
                    Strings.Award,
                    Strings.AwardedTo,
                    Strings.NoteCharacterLimitExceeded,
                    Strings.UnauthorizedAccess,
                    Strings.PreviewBadgeTitle,
                };
                return this.Ok(strings);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetching resource strings.");
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}