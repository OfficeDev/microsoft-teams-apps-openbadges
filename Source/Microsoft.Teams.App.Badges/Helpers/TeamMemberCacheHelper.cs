// <copyright file="TeamMemberCacheHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Teams.App.Badges.Models;
    using Microsoft.Teams.Apps.Scrum.Common;

    /// <summary>
    /// Implements team member cache.
    /// </summary>
    public class TeamMemberCacheHelper : ITeamMemberCacheHelper
    {
        /// <summary>
        /// Sets the team members cache key.
        /// </summary>
        private const string TeamMembersCacheKey = "teamMembersCacheKey";

        /// <summary>
        /// Cache for storing teamMembers information.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMemberCacheHelper"/> class.
        /// </summary>
        /// <param name="memoryCache">MemoryCache instance for caching authorization result.</param>
        public TeamMemberCacheHelper(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Provide team members information.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="teamId">Describes a team Id.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns team members information from cache.</returns>
        public async Task<List<TeamsUserInfo>> GetTeamMembersInfoAsync(ITurnContext turnContext, string teamId, CancellationToken cancellationToken)
        {
            string continuationToken = null;
            bool isCacheEntryExists = this.memoryCache.TryGetValue(TeamMembersCacheKey + teamId, out List<TeamsUserInfo> channelMembers);
            if (!isCacheEntryExists)
            {
                if (channelMembers == null)
                {
                    channelMembers = new List<TeamsUserInfo>();
                }

                do
                {
                    var currentPage = await TeamsInfo.GetPagedTeamMembersAsync(turnContext, teamId, continuationToken, pageSize: 500, cancellationToken);
                    continuationToken = currentPage.ContinuationToken;
                    channelMembers.AddRange(currentPage.Members.Select(member => new TeamsUserInfo { AadObjectId = member.AadObjectId, Email = member.Email, Id = member.Id, Name = member.Name }));
                }
                while (continuationToken != null && channelMembers.Count > 0);
            }

            if (channelMembers.Count > 0)
            {
                this.memoryCache.Set(TeamMembersCacheKey, channelMembers, TimeSpan.FromHours(1));
            }

            return channelMembers;
        }
    }
}
