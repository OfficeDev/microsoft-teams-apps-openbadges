// <copyright file="AwardCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.App.Badges.Models;
    using Microsoft.Teams.App.Badges.Resources;

    /// <summary>
    /// Class having method to create card sent after awarding a badge.
    /// </summary>
    public class AwardCard
    {
        /// <summary>
        /// Get adaptive card attachment when badge is awarded to user.
        /// </summary>
        /// <param name="viewBadge">Instance of class containing information of awarded badge.</param>
        /// <returns>An attachment containing award confirmation card.</returns>
        public static Attachment GetAwardBadgeAttachment(ViewBadge viewBadge)
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
                                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Large,
                                        Wrap = true,
                                        Text = viewBadge.BadgeName,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        Height = AdaptiveHeight.Auto,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new System.Uri(viewBadge.ImageUri),
                                        Size = AdaptiveImageSize.Medium,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Height = AdaptiveHeight.Auto,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Small,
                                        Wrap = true,
                                        Text = string.Format(Strings.AwardedTo, viewBadge.AwardedBy),
                                        Weight = AdaptiveTextWeight.Default,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Small,
                                        Wrap = true,
                                        Text = $"{string.Join(", ", viewBadge.AwardRecipients)}",
                                        Weight = AdaptiveTextWeight.Bolder,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Size = AdaptiveTextSize.Small,
                                        Wrap = true,
                                        Text = viewBadge.Narrative,
                                        Weight = AdaptiveTextWeight.Default,
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
