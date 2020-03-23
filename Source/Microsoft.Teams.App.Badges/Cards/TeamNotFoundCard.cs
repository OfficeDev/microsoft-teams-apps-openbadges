// <copyright file="TeamNotFoundCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.App.Badges.Resources;

    /// <summary>
    /// Class having method to return card when no team is found after invoking task module.
    /// </summary>
    public class TeamNotFoundCard
    {
        /// <summary>
        /// Get team not found card attachment.
        /// </summary>
        /// <returns>An attachment.</returns>
        public static Attachment GetAttachment()
        {
            AdaptiveCard card = new AdaptiveCard("1.2")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Size = AdaptiveTextSize.Default,
                        Wrap = true,
                        Text = Strings.NoTeamFound,
                        Weight = AdaptiveTextWeight.Bolder,
                        Color = AdaptiveTextColor.Attention,
                    },
                },
            };

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
            return adaptiveCardAttachment;
        }
    }
}
