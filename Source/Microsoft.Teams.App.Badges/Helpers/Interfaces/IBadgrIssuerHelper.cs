// <copyright file="IBadgrIssuerHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.App.Badges.Models;

    /// <summary>
    /// Handles API calls for Badgr API to get issuer details based on query.
    /// </summary>
    public interface IBadgrIssuerHelper
    {
        /// <summary>
        /// Add user into Issuer group with "staff" role.
        /// </summary>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <returns>Returns success if user is added in Issuer group successfully.</returns>
        Task<bool> AddUserInIssuerGroupAsync(string requestBody);

        /// <summary>
        /// Assigns "staff" role to user so that user can view and share badges.
        /// </summary>
        /// <param name="userProfile">Badgr user profile for logged in user. </param>
        /// <returns>Returns role assigned to the user.</returns>
        Task<string> AssignUserRoleAsync(UserProfile userProfile);

        /// <summary>
        /// Get details of issuer group from Badge API.
        /// </summary>
        /// <returns>Returns Issuer group details.</returns>
        Task<Issuer> GetIssuerGroupDetailsAsync();

        /// <summary>
        /// Get user role in Badgr API in order to create, award or view badges.
        /// </summary>
        /// <param name="emailId">Email ID used by user to log into Badgr. </param>
        /// <returns>Returns user role in Badgr API.</returns>
        Task<string> GetUserRoleAsync(string emailId);

        /// <summary>
        /// Get issuer groups from Badge API.
        /// </summary>
        /// <param name="ownerToken">Badgr API owner access token.</param>
        /// <returns> Returns issuer groups.</returns>
        Task<List<Issuer>> GetIssuerGroupsAsync();

        /// <summary>
        /// Get issuer ID of user specified issuer group name.
        /// </summary>
        /// <param name="userToken">Badgr API user access token.</param>
        /// <returns>Issuer group ID.</returns>
        Task<string> GetIssuerEntityId();
    }
}