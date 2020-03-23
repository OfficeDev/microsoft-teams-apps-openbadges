// <copyright file="AdapterWithErrorHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges
{
    using System;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.App.Badges.Resources;

    /// <summary>
    /// Class to handle errors and exception occurred in bot.
    /// </summary>
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="telemetryClient">Telemetry client for logging events and errors.</param>
        /// <param name="conversationState">Reads and writes conversation state for your bot to storage.</param>
        public AdapterWithErrorHandler(IConfiguration configuration, TelemetryClient telemetryClient, ConversationState conversationState = null)
            : base(configuration)
        {
            this.OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                telemetryClient.TrackException(exception);

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(Strings.ExceptionResponse);

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception ex)
                    {
                        telemetryClient.TrackException(ex);
                    }
                }
            };
        }
    }
}
