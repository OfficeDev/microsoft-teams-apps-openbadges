// <copyright file="IBadgrUserHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.App.Badges.Models;

    /// <summary>
    /// Handles API calls for Badgr API to get user badge details based on query.
    /// </summary>
    public interface IBadgrUserHelper
    {
        /// <summary>
        /// Get the authenticated badge user’s profile.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <returns>Returns details of the authenticated badge user.</returns>
        Task<UserProfile> GetBadgeUserDetailsAsync(string token = null);

        /// <summary>
        /// Awards badge to multiple users in Issuer group.
        /// </summary>
        /// <param name="assertionDetails">Details of the assertion to be awarded and recipient of the award.</param>
        /// <returns>Returns success/failure on whether badge awarded to multiple users successfully. </returns>
        Task<bool> AwardBadgeToUsersAsync(AssertionDetail assertionDetails);

        /// <summary>
        /// Get all badges created in issuer group.
        /// </summary>
        /// <returns>Returns collection of badges created in Issuer group.</returns>
        Task<IEnumerable<BadgeEntity>> GetAllBadgesAsync();

        /// <summary>
        /// Get all badges earned by user in the backpack by issuer group .
        /// </summary>
        /// <returns>Returns badges earned by user in Issuer group.</returns>
        Task<IEnumerable<EarnedBadgeResponse>> GetEarnedBadgesAsync();

        /// <summary>
        /// Get user access tokens created in Badgr API.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <returns>Returns collection of access tokens for authenticated badge user.</returns>
        Task<IEnumerable<BadgeAccessToken>> GetUserAccessTokenAsync(string token);

        /// <summary>
        /// Revokes access token generated in badgr API.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <param name="badgeTokens">Collection of tokens generated for authenticated user in Badge API. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<bool> RevokeUserAccessTokenAsync(string token, IEnumerable<BadgeAccessToken> badgeTokens);

        /// <summary>
        /// Validates if account used to log into Badgr API is different than the one that was used to authenticate to Teams.
        /// </summary>
        /// <param name="emailId">Email ID used by user to log into Teams. </param>
        /// <param name="token">Badgr API user access token.</param>
        /// <returns>Returns whether email ID used for logging into Teams and Badgr API is same. </returns>
        Task<bool> ValidateUserEmailIdAsync(string emailId, string token);
    }
}