// <copyright file="BadgrUserHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.App.Badges.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles API calls for Badgr API to get user badge details based on query.
    /// </summary>
    public class BadgrUserHelper : IBadgrUserHelper
    {
        /// <summary>
        /// Badgr REST API get call URL for getting information about authenticated badge user who is currently logged in.
        /// </summary>
        private const string GetBadgeUserDetailUrl = "{0}/v2/users/self";

        /// <summary>
        /// Badgr REST API get call URL for getting information about all badges.
        /// </summary>
        private const string GetAllBadgesUrl = "{0}/v2/issuers/{1}/badgeclasses";

        /// <summary>
        /// Badgr REST API get call URL for getting information about earned badges.
        /// </summary>
        private const string GetEarnedBadgeUrl = "{0}/v1/earner/badges";

        /// <summary>
        /// Badgr REST API post call URL for awarding badge to users.
        /// </summary>
        private const string AwardBadgeToUserUrl = "{0}/v1/issuer/issuers/{1}/badges/{2}/batchAssertions";

        /// <summary>
        /// Badgr REST API get call URL for getting Badgr access tokens generated for user.
        /// </summary>
        private const string GetUserAccessTokenUrl = "{0}/v2/auth/tokens";

        /// <summary>
        /// Badgr REST API post call URL for revoking Badgr access tokens generated for user.
        /// </summary>
        private const string RevokeUserAccessTokenUrl = "{0}/v2/auth/tokens/{1}";

        /// <summary>
        /// Badgr API base URL.
        /// </summary>
        private readonly string badgeProviderBaseUrl;

        /// <summary>
        /// Issuer group name.
        /// </summary>
        private readonly string issuerEntityName;

        /// <summary>
        /// Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
        /// </summary>
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Helper to handle errors and get user details.
        /// </summary>
        private readonly IBadgrApiHelper badgrApiHelper;

        /// <summary>
        /// Helper to handle errors and get list of issuer groups.
        /// </summary>
        private readonly IBadgrIssuerHelper badgrIssuerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgrUserHelper"/> class.
        /// </summary>
        /// <param name="client">Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for Bagdr rest API.</param>
        /// <param name="badgrApiHelper">Helper to handle errors and get user details.</param>
        /// <param name="badgrApiHelper">Helper to handle errors and get list of issuer groups.</param>
        public BadgrUserHelper(HttpClient client, ILogger<BadgrApiHelper> logger, IOptionsMonitor<BadgeApiAppSettings> optionsAccessor, IBadgrApiHelper badgrApiHelper, IBadgrIssuerHelper badgrIssuerHelper)
        {
            this.client = client;
            this.logger = logger;
            this.issuerEntityName = optionsAccessor.CurrentValue.IssuerEntityName;
            this.badgeProviderBaseUrl = optionsAccessor.CurrentValue.BaseUrl;
            this.badgrApiHelper = badgrApiHelper;
            this.badgrIssuerHelper = badgrIssuerHelper;
        }

        /// <summary>
        /// Get the authenticated badge user’s profile.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <returns>Returns details of the authenticated badge user.</returns>
        public async Task<UserProfile> GetBadgeUserDetailsAsync(string token = null)
        {
            this.logger.LogInformation("Getting information about badge user.");
            var requestUrl = string.Format(GetBadgeUserDetailUrl, this.badgeProviderBaseUrl);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET");
            if (!string.IsNullOrEmpty(token))
            {
                request = this.GetHttpRequestMessageData(requestUrl, "GET", string.Empty, token);
            }

            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            if (result == null)
            {
                this.logger.LogError("No user profile received from Badgr API service.");
                throw new Exception("No user profile received from Badgr API service.");
            }

            var userProfile = JsonConvert.DeserializeObject<IEnumerable<UserProfile>>(JObject.Parse(result).SelectToken("result").ToString()).FirstOrDefault();
            if (userProfile != null)
            {
                this.logger.LogInformation("Received information about badge user successfully.");
                return userProfile;
            }
            else
            {
                this.logger.LogError("Error in retrieving user profile.");
                throw new Exception("Error in retrieving user profile.");
            }
        }

        /// <summary>
        /// Validates if account used to log into Badgr API is different than the one that was used to authenticate to Teams.
        /// </summary>
        /// <param name="emailId">Email ID used by user to log into Teams. </param>
        /// <param name="token">Badgr API user access token.</param>
        /// <returns>Returns whether email ID used for logging into Teams and Badgr API is same. </returns>
        public async Task<bool> ValidateUserEmailIdAsync(string emailId, string token)
        {
            this.logger.LogInformation("User logged in ID validation started.");
            var userProfile = await this.GetBadgeUserDetailsAsync(token);

            // Validate if the user has signed in with the same Azure Active Directory account which was used to authenticate to Teams.
            var emailDetails = userProfile.Emails?.FirstOrDefault(email => email.Email.Equals(emailId, StringComparison.OrdinalIgnoreCase));
            if (emailDetails != null)
            {
                this.logger.LogInformation("User found in Badgr with same email used to log in to Teams.");
                return true;
            }

            this.logger.LogInformation("User not found in Badgr with same email used to log in to Teams.");
            return false;
        }

        /// <summary>
        /// Get all badges created in issuer group.
        /// </summary>
        /// <returns>Returns collection of badges created in Issuer group.</returns>
        public async Task<IEnumerable<BadgeEntity>> GetAllBadgesAsync()
        {
            this.logger.LogInformation("Getting information about all badges created in Issuer group.");
            var entityId = await this.badgrIssuerHelper.GetIssuerEntityId();
            if (entityId == null)
            {
                throw new Exception("Entity ID cannot be retrieved.");
            }

            var requestUrl = string.Format(GetAllBadgesUrl, this.badgeProviderBaseUrl, entityId);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            var badgeClasses = JsonConvert.DeserializeObject<IEnumerable<BadgeEntity>>(JObject.Parse(result).SelectToken("result").ToString()).OrderByDescending(badgeClass => badgeClass.CreatedAt);
            this.logger.LogInformation("Received information about all badges created in Issuer group successfully.");
            return badgeClasses;
        }

        /// <summary>
        /// Get all badges earned by user in the backpack by issuer group .
        /// </summary>
        /// <returns>Returns badges earned by user in Issuer group.</returns>
        public async Task<IEnumerable<EarnedBadgeResponse>> GetEarnedBadgesAsync()
        {
            this.logger.LogInformation("Getting information about badges earned by user in backpack for Issuer group.");
            var requestUrl = string.Format(GetEarnedBadgeUrl, this.badgeProviderBaseUrl);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            if (result == null)
            {
                this.logger.LogInformation("No earned badges found for user.");
                return null;
            }

            this.logger.LogInformation("Mapping complex JSON response to simple entity EarnedBadgeResponse");

            var entityId = await this.badgrIssuerHelper.GetIssuerEntityId();
            if (entityId == null)
            {
                throw new Exception("Entity ID cannot be retrieved.");
            }

            // Get earned badges in particular Issuer group.
            var earnedBadges = JsonConvert.DeserializeObject<IEnumerable<EarnedBadge>>(JArray.Parse(result).ToString())?
                                .Where(earnedBadge => earnedBadge.EarnedBadgeDetail.BadgeDetail.Issuer.Id.Contains($"/{entityId}", StringComparison.OrdinalIgnoreCase))
                                .Select(badge => new EarnedBadgeResponse
                                {
                                    Name = badge.EarnedBadgeDetail.BadgeDetail.Name.Value,
                                    Description = badge.EarnedBadgeDetail.BadgeDetail.Description.Value,
                                    ImageUri = badge.ImageUri,
                                    AwardedBy = badge.EarnedBadgeDetail.BadgeDetail.Issuer.Name.Value,
                                    AwardedOn = Convert.ToDateTime(badge.EarnedBadgeDetail.IssuedOn.Value, CultureInfo.InvariantCulture),
                                });

            return earnedBadges;
        }

        /// <summary>
        /// Awards badge to multiple users in Issuer group.
        /// </summary>
        /// <param name="assertionDetails">Details of the assertion to be awarded and recipient of the award.</param>
        /// <returns>Returns success/failure on whether badge awarded to multiple users successfully. </returns>
        public async Task<bool> AwardBadgeToUsersAsync(AssertionDetail assertionDetails)
        {
            this.logger.LogInformation("Adding user into Issuer group started.");
            var entityId = await this.badgrIssuerHelper.GetIssuerEntityId();
            if (entityId == null)
            {
                throw new Exception("Entity ID cannot be retrieved.");
            }

            var requestUrl = string.Format(AwardBadgeToUserUrl, this.badgeProviderBaseUrl, entityId, assertionDetails.BadgeClassId);
            string requestBody = JsonConvert.SerializeObject(assertionDetails);
            var request = this.GetHttpRequestMessageData(requestUrl, "POST", requestBody);
            var response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                this.logger.LogInformation("Awarded badge to users successfully.");
                return true;
            }

            var result = await response.Content.ReadAsStringAsync();
            this.badgrApiHelper.HandleError(response, result);
            return false;
        }

        /// <summary>
        /// Get user access tokens created in Badgr API.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <returns>Returns collection of access tokens for authenticated badge user.</returns>
        public async Task<IEnumerable<BadgeAccessToken>> GetUserAccessTokenAsync(string token)
        {
            var requestUrl = string.Format(GetUserAccessTokenUrl, this.badgeProviderBaseUrl);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET", string.Empty, token);

            this.logger.LogInformation("Getting user access tokens for user.");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            if (result == null)
            {
                this.logger.LogError("Tokens not found for Badgr API.");
                throw new Exception("Tokens not found for Badgr API.");
            }

            var tokens = JsonConvert.DeserializeObject<IEnumerable<BadgeAccessToken>>(JObject.Parse(result).SelectToken("result").ToString());
            this.logger.LogInformation("Getting user access tokens for user completed.");
            return tokens;
        }

        /// <summary>
        /// Revokes access token generated in badgr API.
        /// </summary>
        /// <param name="token">Badgr API user access token. </param>
        /// <param name="badgeTokens">Collection of tokens generated for authenticated user in Badge API. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> RevokeUserAccessTokenAsync(string token, IEnumerable<BadgeAccessToken> badgeTokens)
        {
            this.logger.LogInformation("Revoke user access token started.");

            if (badgeTokens == null || !badgeTokens.Any())
            {
                this.logger.LogError("No tokens received to revoke.");
                throw new Exception("No tokens received to revoke.");
            }

            foreach (var badgeToken in badgeTokens)
            {
                var requestUrl = string.Format(RevokeUserAccessTokenUrl, this.badgeProviderBaseUrl, badgeToken.EntityId);
                var request = this.GetHttpRequestMessageData(requestUrl, "DELETE", string.Empty, token);
                var response = await this.client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    this.logger.LogInformation("User access token revoked.");
                    continue;
                }

                var result = await response.Content.ReadAsStringAsync();
                this.badgrApiHelper.HandleError(response, result);
            }

            return true;
        }

        /// <summary>
        /// Creates HTTP request object for Badgr API call.
        /// </summary>
        /// <param name="requestUrl">Badgr API request Uri for GET call.</param>
        /// <param name="httpMethodType">HTTP method type for making API request.</param>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <param name="token">Badgr user access token passed from bot.</param>
        /// <returns>Returns HTTP request object for APi calls. </returns>
        private HttpRequestMessage GetHttpRequestMessageData(string requestUrl, string httpMethodType, string requestBody = "", string token = null)
        {
            this.logger.LogInformation("Creating HTTP request message object for Badgr API calls.");
            HttpMethod httpMethod = new HttpMethod(httpMethodType);
            var request = new HttpRequestMessage(httpMethod, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
            };

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            this.logger.LogInformation("Adding HTTP request headers.");
            this.logger.LogInformation("Created HTTP request object for Badgr API calls.");
            return request;
        }
    }
}
