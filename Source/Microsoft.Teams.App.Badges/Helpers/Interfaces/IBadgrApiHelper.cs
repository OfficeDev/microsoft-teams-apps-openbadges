// <copyright file="IBadgrApiHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles API calls for Badgr API to get issuer groups.
    /// </summary>
    public interface IBadgrApiHelper
    {
        /// <summary>
        /// Get owner access token to perform CRUD operations on Issuer group.
        /// </summary>
        /// <param name="username">Owner user name retrieved from Azure Key Vault.</param>
        /// <param name="password">Owner password retrieved from Azure Key Vault.</param>
        /// <returns>Returns owner token from Badge API.</returns>
        Task<string> GetOwnerAccessTokenAsync(string username, string password);

        /// <summary>
        /// Creates HTTP request object for Badgr API call.
        /// </summary>
        /// <param name="requestUrl">Badgr API request Uri for GET call.</param>
        /// <param name="token">Badgr API user access token.</param>
        /// <param name="httpMethodType">HTTP method type for making API request.</param>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <returns>Returns HTTP request object for APi calls. </returns>
        HttpRequestMessage GetHttpRequestMessageData(string requestUrl, string token, string httpMethodType, string requestBody = "");

        /// <summary>
        /// Handles error occurred from Badgr API as per the response status code and throws exception to parent.
        /// </summary>
        /// <param name="response">HTTP Response received from Badgr API call.</param>
        /// <param name="result">Result string received from Badgr API call response.</param>
        void HandleError(HttpResponseMessage response, string result);
    }
}