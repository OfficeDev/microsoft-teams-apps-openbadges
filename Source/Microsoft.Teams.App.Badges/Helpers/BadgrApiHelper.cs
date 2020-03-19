// <copyright file="BadgrApiHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
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
    /// Handles API calls for Badgr API to get issuer groups.
    /// </summary>
    public class BadgrApiHelper : IBadgrApiHelper
    {
        /// <summary>
        /// Badgr REST API get call URL for getting information about owner access token.
        /// </summary>
        private const string GetOwnerAccessTokenUrl = "{0}/o/token";

        /// <summary>
        /// Badgr API base URL.
        /// </summary>
        private readonly string badgeProviderBaseUrl;

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
        /// Initializes a new instance of the <see cref="BadgrApiHelper"/> class.
        /// Handles API calls for Badge API to create and award badges, view earned badges.
        /// </summary>
        /// <param name="client">Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for Bagdr rest API.</param>
        public BadgrApiHelper(HttpClient client, ILogger<BadgrApiHelper> logger, IOptionsMonitor<BadgeApiAppSettings> optionsAccessor)
        {
            this.client = client;
            this.logger = logger;
            this.badgeProviderBaseUrl = optionsAccessor.CurrentValue.BaseUrl;
        }

        /// <summary>
        /// Creates HTTP request object for Badgr API call.
        /// </summary>
        /// <param name="requestUrl">Badgr API request Uri for GET call.</param>
        /// <param name="token">Badgr API user access token.</param>
        /// <param name="httpMethodType">HTTP method type for making API request.</param>
        /// <param name="requestBody">Badgr API request body.</param>
        /// <returns>Returns HTTP request object for APi calls. </returns>
        public HttpRequestMessage GetHttpRequestMessageData(string requestUrl, string token, string httpMethodType, string requestBody = "")
        {
            this.logger.LogInformation("Creating HTTP request message object for Badgr API calls.");
            HttpMethod httpMethod = new HttpMethod(httpMethodType);
            var request = new HttpRequestMessage(httpMethod, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
            };

            this.logger.LogInformation("Adding HTTP request headers.");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            this.logger.LogInformation("Created HTTP request object for Badgr API calls.");
            return request;
        }

        /// <summary>
        /// Get owner access token to perform CRUD operations on Issuer group.
        /// </summary>
        /// <param name="username">Owner user name retrieved from Azure Key Vault.</param>
        /// <param name="password">Owner password retrieved from Azure Key Vault.</param>
        /// <returns>Returns owner token from Badge API.</returns>
        public async Task<string> GetOwnerAccessTokenAsync(string username, string password)
        {
            var requestUrl = string.Format(GetOwnerAccessTokenUrl, this.badgeProviderBaseUrl);
            var request = this.GetHttpRequestMessageData(requestUrl, null, "POST");

            var parameters = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
            };
            request.Content = new FormUrlEncodedContent(parameters);

            this.logger.LogInformation("Getting owner access token from Badgr API.");
            var response = await this.client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                this.HandleError(response, result);
            }

            var tokenInformation = JsonConvert.DeserializeObject<OwnerAccessToken>(JObject.Parse(result).ToString());
            if (tokenInformation != null)
            {
                this.logger.LogInformation("Retrieved owner access token from badgr API.");
                return tokenInformation.Token;
            }
            else
            {
                this.logger.LogError("Error in retrieving owner access token for Badgr API.");
                throw new Exception("Error in retrieving owner access token for Badgr API.");
            }
        }

        /// <summary>
        /// Handles error occurred from Badgr API as per the response status code and throws exception to parent.
        /// </summary>
        /// <param name="response">HTTP Response received from Badgr API call.</param>
        /// <param name="result">Result string received from Badgr API call response.</param>
        public void HandleError(HttpResponseMessage response, string result)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    this.logger.LogError($"Invalid badgr user access token. Error: {response.ReasonPhrase} - {result}");
                    throw new UnauthorizedAccessException("Invalid badgr user access token");
                case HttpStatusCode.NotFound:
                    this.logger.LogError($"Badgr API call failed. URL not found. Error: {response.ReasonPhrase} - {result}");
                    throw new HttpRequestException("Badgr API call failed. URL not found.");
                default:
                    this.logger.LogError($"Error: {response.ReasonPhrase} - {result}");
                    throw new Exception($"{response.ReasonPhrase} - {result}");
            }
        }
    }
}