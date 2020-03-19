// <copyright file="ITokenHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Helper for custom JWT token generation and retrieval of user Badgr access token.
    /// </summary>
    public interface ITokenHelper
    {
        /// <summary>
        /// Generate JWT token used by client app to authenticate HTTP calls with API.
        /// </summary>
        /// <param name="serviceURL">Service URL from bot.</param>
        /// <param name="fromId">Unique Id from activity.</param>
        /// <param name="jwtExpiryMinutes">Expiry of token.</param>
        /// <returns>JWT token.</returns>
        string GenerateInternalAPIToken(string serviceURL, string fromId, int jwtExpiryMinutes);

        /// <summary>
        /// Get Badgr access token for user.
        /// </summary>
        /// <param name="fromId">Activity.From.Id from bot.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<string> GetBadgrTokenAsync(string fromId);
    }
}