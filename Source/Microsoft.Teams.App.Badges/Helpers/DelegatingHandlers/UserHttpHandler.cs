// <copyright file="UserHttpHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers.DelegatingHandlers
{
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Teams.App.Badges.Helpers;
    using Microsoft.Teams.App.Badges.Models;

    /// <summary>
    /// Delegating handler to get user Badgr access token and add it to HTTP request header.
    /// </summary>
    public class UserHttpHandler : DelegatingHandler
    {
        private IHttpContextAccessor httpContextAccessor;
        private ITokenHelper tokenHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserHttpHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor for getting user claims.</param>
        /// <param name="tokenHelper">Token helper to get Badgr user access token.</param>
        public UserHttpHandler(IHttpContextAccessor httpContextAccessor, ITokenHelper tokenHelper)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.tokenHelper = tokenHelper;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Get user Badgr access token from Microsoft Bot Framework.
            var userClaims = this.GetUserClaims();

            // Claims will be null in case if helper method is called from BadgeBot.cs
            if (!string.IsNullOrEmpty(userClaims.FromId))
            {
                var userToken = await this.tokenHelper.GetBadgrTokenAsync(userClaims.FromId);

                // Add Badgr access token in header for Badgr API calls.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            }

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Get claims of user.
        /// </summary>
        /// <returns>User claims.</returns>
        private JwtClaims GetUserClaims()
        {
            var claims = this.httpContextAccessor.HttpContext.User.Claims;
            var jwtClaims = new JwtClaims
            {
                FromId = claims?.Where(claim => claim.Type == "fromId").Select(claim => claim.Value).FirstOrDefault(),
                ServiceUrl = claims?.Where(claim => claim.Type == "serviceURL").Select(claim => claim.Value).FirstOrDefault(),
            };

            return jwtClaims;
        }
    }
}
