// <copyright file="BadgrIssuerHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.App.Badges.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles API calls for Badgr API to get issuer details based on query.
    /// </summary>
    public class BadgrIssuerHelper : IBadgrIssuerHelper
    {
        /// <summary>
        /// Badgr REST API get call URL for getting issuer groups.
        /// </summary>
        private const string GetIssuerGroupsUrl = "{0}/v2/issuers";

        /// <summary>
        /// Describes user role "staff" in Badge API.
        /// </summary>
        private const string Staff = "staff";

        /// <summary>
        /// Describes action to add "staff" in Badge API.
        /// </summary>
        private const string AddAction = "add";

        /// <summary>
        /// Badgr REST API get call URL for getting information about Issuer group.
        /// </summary>
        private const string GetIssuerGroupDetailsUrl = "{0}/v2/issuers/{1}";

        /// <summary>
        /// Badgr REST API post call URL for adding user in Issuer group.
        /// </summary>
        private const string AddUserInIssuerGroupUrl = "{0}/v1/issuer/issuers/{1}/staff";

        /// <summary>
        /// Unique identifier of the entity in which user needs to be searched or added (in this case Issuer group ID).
        /// </summary>
        private static string entityId = null;

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
        /// Helper to handle errors and get list of issuer groups.
        /// </summary>
        private readonly IBadgrApiHelper badgrApiHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgrIssuerHelper"/> class.
        /// </summary>
        /// <param name="client">Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for Bagdr rest API.</param>
        /// <param name="badgrApiHelper">Helper to handle errors and get list of issuer groups.</param>
        public BadgrIssuerHelper(HttpClient client, ILogger<BadgrApiHelper> logger, IOptionsMonitor<BadgeApiAppSettings> optionsAccessor, IBadgrApiHelper badgrApiHelper)
        {
            this.client = client;
            this.logger = logger;
            this.issuerEntityName = optionsAccessor.CurrentValue.IssuerEntityName;
            this.badgeProviderBaseUrl = optionsAccessor.CurrentValue.BaseUrl;
            this.badgrApiHelper = badgrApiHelper;
        }

        /// <summary>
        /// Get user role in Badgr API in order to create, award or view badges.
        /// </summary>
        /// <param name="emailId">Email ID used by user to log into Badgr. </param>
        /// <returns>Returns user role in Badgr API.</returns>
        public async Task<string> GetUserRoleAsync(string emailId)
        {
            this.logger.LogInformation("Getting user role in Issuer group.");

            // Get information about Issuer group
            var issuerDetails = await this.GetIssuerGroupDetailsAsync();
            if (issuerDetails == null)
            {
                this.logger.LogError("Issuer group not found.");
                throw new Exception("Issuer group not found.");
            }

            // check if user is part of Issuer group
            var staffDetails = issuerDetails.Staff?.FirstOrDefault(staff => staff.UserProfile.Emails.Any(email => email.Email.Equals(emailId, StringComparison.OrdinalIgnoreCase)));
            if (staffDetails != null)
            {
                this.logger.LogInformation($"User found in Issuer group with role {staffDetails.Role}");
                return staffDetails.Role;
            }
            else
            {
                this.logger.LogInformation("User not found in Issuer group.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Assigns "staff" role to user so that user can view and share badges.
        /// </summary>
        /// <param name="userProfile">Badgr user profile for logged in user. </param>
        /// <returns>Returns role assigned to the user.</returns>
        public async Task<string> AssignUserRoleAsync(UserProfile userProfile)
        {
            this.logger.LogInformation("Assigning 'staff' role to user in Issuer group.");

            // Get primary email Id of user and add in Issuer staff.
            var emailDetails = userProfile.Emails?.FirstOrDefault(email => email.Primary);

            if (emailDetails == null)
            {
                this.logger.LogError("User cannot be added in Issuer group because no primary email Id exists of user.");
                throw new Exception("User cannot be added in Issuer group because no primary email Id exists of user.");
            }

            var requestPayload = JsonConvert.SerializeObject(new
            {
                action = AddAction,
                email = emailDetails.Email,
                role = Staff,
            });

            // Add user information into Issuer group with "staff" role.
            var isAddUserSuccess = await this.AddUserInIssuerGroupAsync(requestPayload);
            if (!isAddUserSuccess)
            {
                this.logger.LogError("Error while adding user to Issuer group.");
                throw new Exception("Error while adding user to Issuer group.");
            }

            this.logger.LogInformation("Assigned 'staff' role to user in Issuer group.");
            return Staff;
        }

        /// <summary>
        /// Get details of issuer group from Badge API.
        /// </summary>
        /// <returns>Returns Issuer group details.</returns>
        public async Task<Issuer> GetIssuerGroupDetailsAsync()
        {
            this.logger.LogInformation("Getting information about Issuer group.");
            var entityId = await this.GetIssuerEntityId();
            if (entityId == null)
            {
                throw new Exception("Entity ID cannot be retrieved.");
            }

            var requestUrl = string.Format(GetIssuerGroupDetailsUrl, this.badgeProviderBaseUrl, entityId);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            if (result == null)
            {
                this.logger.LogError("Issuer group not found.");
                throw new Exception("Issuer group not found.");
            }

            var issuerDetails = JsonConvert.DeserializeObject<IEnumerable<Issuer>>(JObject.Parse(result).SelectToken("result").ToString()).FirstOrDefault();
            this.logger.LogInformation("Received information about Issuer group successfully.");
            return issuerDetails;
        }

        /// <summary>
        /// Add user into Issuer group with "staff" role.
        /// </summary>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <returns>Returns success if user is added in Issuer group successfully.</returns>
        public async Task<bool> AddUserInIssuerGroupAsync(string requestBody)
        {
            this.logger.LogInformation("Adding user into Issuer group started.");
            var entityId = await this.GetIssuerEntityId();
            if (entityId == null)
            {
                throw new Exception("Entity ID cannot be retrieved.");
            }

            var requestUrl = string.Format(AddUserInIssuerGroupUrl, this.badgeProviderBaseUrl, entityId);
            var request = this.GetHttpRequestMessageData(requestUrl, "POST", requestBody);
            var response = await this.client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                this.logger.LogInformation("Added user into Issuer group.");
                return true;
            }

            var result = await response.Content.ReadAsStringAsync();
            this.badgrApiHelper.HandleError(response, result);
            return false;
        }

        /// <summary>
        /// Get issuer ID of user specified issuer group name.
        /// </summary>
        /// <returns>Issuer group ID.</returns>
        public async Task<string> GetIssuerEntityId()
        {
            if (entityId == null)
            {
                var issuerGroups = await this.GetIssuerGroupsAsync();
                entityId = issuerGroups?.FirstOrDefault(issuer => issuer.Name.Equals(this.issuerEntityName))?.EntityId;
            }

            return entityId;
        }

        /// <summary>
        /// Get issuer groups from Badge API.
        /// </summary>
        /// <param name="ownerToken">Badge API owner access token.</param>
        /// <returns> Returns issuer groups.</returns>
        public async Task<List<Issuer>> GetIssuerGroupsAsync()
        {
            this.logger.LogInformation("Getting issuer groups.");

            var requestUrl = string.Format(GetIssuerGroupsUrl, this.badgeProviderBaseUrl);
            var request = this.GetHttpRequestMessageData(requestUrl, "GET");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.badgrApiHelper.HandleError(response, result);
            }

            if (result == null)
            {
                this.logger.LogError("Issuer groups not found.");
                throw new Exception("Issuer groups not found.");
            }

            var issuerGroups = JsonConvert.DeserializeObject<IEnumerable<Issuer>>(JObject.Parse(result).SelectToken("result").ToString()).ToList();
            this.logger.LogInformation("Received information about issuer groups successfully.");
            return issuerGroups;
        }

        /// <summary>
        /// Creates HTTP request object for Badgr API call.
        /// </summary>
        /// <param name="requestUrl">Badgr API request Uri for GET call.</param>
        /// <param name="httpMethodType">HTTP method type for making API request.</param>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <returns>Returns HTTP request object for APi calls. </returns>
        private HttpRequestMessage GetHttpRequestMessageData(string requestUrl, string httpMethodType, string requestBody = "")
        {
            this.logger.LogInformation("Creating HTTP request message object for Badgr API calls.");
            HttpMethod httpMethod = new HttpMethod(httpMethodType);
            var request = new HttpRequestMessage(httpMethod, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
            };

            this.logger.LogInformation("Created HTTP request object for Badgr API calls.");
            return request;
        }
    }
}
