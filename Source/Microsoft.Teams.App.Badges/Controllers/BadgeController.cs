// <copyright file="BadgeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.App.Badges.Helpers;
    using Microsoft.Teams.App.Badges.Models;
    using Error = Microsoft.Teams.App.Badges.Models.Error;

    /// <summary>
    /// Controller to handle Badge API operations.
    /// </summary>
    [Route("api/badges")]
    [ApiController]
    [Authorize]
    public class BadgeController : ControllerBase
    {
        /// <summary>
        /// Microsoft Azure Key Vault base Uri.
        /// </summary>
        private readonly string keyVaultBaseUrl;

        /// <summary>
        /// Secret name for super user name from Microsoft Azure Key Vault.
        /// </summary>
        private readonly string superUserNameKey;

        /// <summary>
        /// Secret name for super user password from Microsoft Azure Key Vault.
        /// </summary>
        private readonly string superUserPasswordKey;

        /// <summary>
        /// Microsoft app ID.
        /// </summary>
        private readonly string appId;

        /// <summary>
        /// Bot adapter to get context.
        /// </summary>
        private readonly BotFrameworkAdapter botAdapter;

        /// <summary>
        /// Generating custom JWT token and retrieving Badgr access token for user.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Instance of badge user helper to create and award badges, view earned badges.
        /// </summary>
        private readonly IBadgrUserHelper badgeUserHelper;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Represents a set of key/value super user configuration for Badges bot.
        /// </summary>
        private readonly SuperUserSettings superUserSettings;

        /// <summary>
        /// Instance of key vault helper to retrieve key vault secrets.
        /// </summary>
        private readonly IKeyVaultHelper keyVaultHelper;

        /// <summary>
        /// Instance of badge Issuer helper to update Issuer and get information of Issuer.
        /// </summary>
        private readonly IBadgrIssuerHelper badgeIssuerHelper;

        /// <summary>
        /// Helper to handle errors and get user details.
        /// </summary>
        private readonly IBadgrApiHelper badgrApiHelper;

        /// <summary>
        /// Helper to get user details from Badgr.
        /// </summary>
        private readonly IBadgrUserHelper badgrUserHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeController"/> class.
        /// </summary>
        /// <param name="botAdapter">Open badges bot adapter.</param>
        /// <param name="badgeUserHelper">Instance of badge user helper.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="tokenHelper">Generating custom JWT token and retrieving Badgr access token for user.</param>
        /// <param name="microsoftAppCredentials">App credentials for Bot/ME.</param>
        /// <param name="superUserSettings">A set of key/value super user configuration for Badges app.</param>
        /// <param name="keyVaultHelper">Instance of key vault helper to retrieve secrets from Microsoft Azure Key Vault.</param>
        /// <param name="badgeIssuerHelper">Instance of badge Issuer helper.</param>
        /// <param name="badgrApiHelper">Helper to handle errors and get user details.</param>
        /// <param name="badgrUserHelper">Helper to get user details from Badgr.</param>
        public BadgeController(
            BotFrameworkAdapter botAdapter,
            IBadgrUserHelper badgeUserHelper,
            ILogger<BadgeController> logger,
            ITokenHelper tokenHelper,
            MicrosoftAppCredentials microsoftAppCredentials,
            IOptionsMonitor<SuperUserSettings> superUserSettings,
            IKeyVaultHelper keyVaultHelper,
            IBadgrIssuerHelper badgeIssuerHelper,
            IBadgrApiHelper badgrApiHelper,
            IBadgrUserHelper badgrUserHelper)
        {
            this.badgeUserHelper = badgeUserHelper;
            this.logger = logger;
            this.tokenHelper = tokenHelper;
            this.botAdapter = botAdapter;
            this.appId = microsoftAppCredentials.MicrosoftAppId;

            if (superUserSettings == null || superUserSettings.CurrentValue == null)
            {
                throw new Exception("Unable to fetch super user settings from configuration file.");
            }

            this.superUserSettings = superUserSettings.CurrentValue;
            this.keyVaultBaseUrl = this.superUserSettings.BaseUrl;
            this.superUserNameKey = this.superUserSettings.SuperUserNameKey;
            this.superUserPasswordKey = this.superUserSettings.SuperUserPasswordKey;

            this.keyVaultHelper = keyVaultHelper;
            this.badgeIssuerHelper = badgeIssuerHelper;
            this.badgrApiHelper = badgrApiHelper;
            this.badgrUserHelper = badgrUserHelper;
        }

        /// <summary>
        /// Get list of members present in a team.
        /// </summary>
        /// <param name="teamId">Team Id for list of members.</param>
        /// <returns>List of members in team.</returns>
        [Route("teammembers")]
        public async Task<IActionResult> GetTeamMembersAsync(string teamId)
        {
            try
            {
                if (teamId == null)
                {
                    return this.BadRequest(new { message = "Team ID cannot be empty." });
                }

                var userClaims = this.GetUserClaims();

                IEnumerable<TeamsChannelAccount> teamsChannelAccounts = new List<TeamsChannelAccount>();
                var conversationReference = new ConversationReference
                {
                    ChannelId = teamId,
                    ServiceUrl = userClaims.ServiceUrl,
                };

                await this.botAdapter.ContinueConversationAsync(
                    this.appId,
                    conversationReference,
                    async (context, token) =>
                    {
                        teamsChannelAccounts = await TeamsInfo.GetTeamMembersAsync(context, teamId, default);
                    },
                    default);

                return this.Ok(teamsChannelAccounts.Select(member => new { content = member.Email, header = member.Name }));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while getting team member list.");
                throw;
            }
        }

        /// <summary>
        /// Get call to retrieve list of all badges created in Issuer group.
        /// </summary>
        /// <param name="email">User's Teams account email ID.</param>
        /// <returns>Returns collection of all badges created in Issuer group.</returns>
        [Route("allbadges")]
        public async Task<IActionResult> GetAllBadges(string email)
        {
            try
            {
                var userBadgrRole = await this.AssignBadgrUserRoleAsync(email);
                if (string.IsNullOrEmpty(userBadgrRole))
                {
                    this.logger.LogError("Failed to fetch or add user to role.");
                    return this.GetErrorResponse(StatusCodes.Status400BadRequest, "Failed to fetch or add user to role.");
                }

                var allBadges = await this.badgeUserHelper.GetAllBadgesAsync();

                this.logger.LogInformation("Call to badge service succeeded");
                return this.Ok(new { allBadges, userBadgrRole });
            }
            catch (UnauthorizedAccessException ex)
            {
                this.logger.LogError(ex, "Failed to get user Badgr token to make call to API.");
                return this.GetErrorResponse(StatusCodes.Status401Unauthorized, "Badgr access token for user is found empty.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to badge service.");
                return this.GetErrorResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get call to retrieve list of badges earned by the user in Issuer group.
        /// </summary>
        /// <returns>Returns collection of earned badges created in Issuer group.</returns>
        [Route("earnedbadges")]
        [HttpGet]
        public async Task<IActionResult> GetEarnedBadges()
        {
            try
            {
                var earnedBadges = await this.badgeUserHelper.GetEarnedBadgesAsync();

                this.logger.LogInformation("Call to badge service succeeded");
                return this.Ok(earnedBadges);
            }
            catch (UnauthorizedAccessException ex)
            {
                this.logger.LogError(ex, "Failed to get user token to make call to API.");
                return this.GetErrorResponse(StatusCodes.Status401Unauthorized, "Badgr access token for user is found empty.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to badge service.");
                return this.GetErrorResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Post call to award badge to multiple user.
        /// </summary>
        /// <param name="assertionDetails">Recipient and award details.</param>
        /// <returns>Returns true for successful operation.</returns>
        [Route("awardbadge")]
        [HttpPost]
        public async Task<IActionResult> AwardBadgeToUsersAsync([FromBody] AssertionDetail assertionDetails)
        {
            try
            {
                if (assertionDetails == null)
                {
                    return this.BadRequest(new { message = "Details for awarding badge cannot be empty." });
                }

                return this.Ok(await this.badgeUserHelper.AwardBadgeToUsersAsync(assertionDetails));
            }
            catch (UnauthorizedAccessException ex)
            {
                this.logger.LogError(ex, "Failed to get user token to make call to API.");
                return this.GetErrorResponse(StatusCodes.Status401Unauthorized, "Badgr access token for user is found empty.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to badge service.");
                return this.GetErrorResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Assign user staff role if its first time sign in.
        /// </summary>
        /// <param name="userTeamsEmail">Logged in user's Teams account email ID.</param>
        /// <returns>Returns user role.</returns>
        private async Task<string> AssignBadgrUserRoleAsync(string userTeamsEmail)
        {
            try
            {
                // If user email ID matches with email used for login in Badgr, then check if user has any role in issuer group.
                var userRoleInBadgr = await this.badgeIssuerHelper.GetUserRoleAsync(userTeamsEmail);

                if (string.IsNullOrEmpty(userRoleInBadgr))
                {
                    // If user is not part of Issuer group, then add user in issuer group and assign "staff" role to user.
                    var userProfile = await this.badgrUserHelper.GetBadgeUserDetailsAsync();

                    userRoleInBadgr = await this.badgeIssuerHelper.AssignUserRoleAsync(userProfile);
                }

                return userRoleInBadgr;
            }
            catch (UnauthorizedAccessException ex)
            {
                this.logger.LogError(ex, "Failed to get user token to make call to API.");
                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to badge service.");
                return null;
            }
        }

        /// <summary>
        /// Get claims of user.
        /// </summary>
        /// <returns>User claims.</returns>
        private JwtClaims GetUserClaims()
        {
            var claims = this.User.Claims;
            var jwtClaims = new JwtClaims
            {
                FromId = claims.Where(claim => claim.Type == "fromId").Select(claim => claim.Value).First(),
                ServiceUrl = claims.Where(claim => claim.Type == "serviceURL").Select(claim => claim.Value).First(),
            };

            return jwtClaims;
        }

        /// <summary>
        /// Creates the error response as per the status codes in case of error.
        /// </summary>
        /// <param name="statusCode">Describes the type of error.</param>
        /// <param name="errorMessage">Describes the error message.</param>
        /// <returns>Returns error response with appropriate message and status code.</returns>
        private IActionResult GetErrorResponse(int statusCode, string errorMessage)
        {
            switch (statusCode)
            {
                case StatusCodes.Status401Unauthorized:
                    return this.StatusCode(
                      StatusCodes.Status401Unauthorized,
                      new Error
                      {
                          StatusCode = "signinRequired",
                          ErrorMessage = errorMessage,
                      });
                case StatusCodes.Status400BadRequest:
                    return this.StatusCode(
                      StatusCodes.Status400BadRequest,
                      new Error
                      {
                          StatusCode = "badRequest",
                          ErrorMessage = errorMessage,
                      });
                default:
                    return this.StatusCode(
                      StatusCodes.Status500InternalServerError,
                      new Error
                      {
                          StatusCode = "internalServerError",
                          ErrorMessage = errorMessage,
                      });
            }
        }
    }
}
