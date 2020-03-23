// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.App.Badges.Resources;

    /// <summary>
    /// Class having method to return welcome card attachment.
    /// </summary>
    public class WelcomeCard
    {
        /// <summary>
        /// Get welcome card attachment.
        /// </summary>
        /// <param name="welcomeCardImageUrl">Welcome card image URL.</param>
        /// <returns>An attachment.</returns>
        public static Attachment GetWelcomeCardAttachment(Uri welcomeCardImageUrl)
        {
            AdaptiveCard card = new AdaptiveCard("1.2")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = welcomeCardImageUrl,
                                        Size = AdaptiveImageSize.Large,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Large,
                                        Wrap = true,
                                        Text = Strings.WelcomeCardTitle,
                                        Weight = AdaptiveTextWeight.Bolder,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Default,
                                        Wrap = true,
                                        Text = Strings.WelcomeCardContent,
                                    },
                                },
                            },
                        },
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
