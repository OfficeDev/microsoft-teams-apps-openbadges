// <copyright file="BadgeBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.App.Badges.Cards;
    using Microsoft.Teams.App.Badges.Helpers;
    using Microsoft.Teams.App.Badges.Models;
    using Microsoft.Teams.App.Badges.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements the core logic of the Badges bot.
    /// </summary>
    public class BadgeBot : TeamsActivityHandler
    {
        /// <summary>
        /// Messaging extension authentication type.
        /// </summary>
        private const string MessagingExtensionAuthType = "auth";

        /// <summary>
        /// Application base Uri.
        /// </summary>
        private readonly string appBaseUrl;

        /// <summary>
        /// OAuth 2.0 bot connection name.
        /// </summary>
        private readonly string connectionName;

        /// <summary>
        /// Application Insights instrumentation key needed for initialization of logger in client application.
        /// </summary>
        private readonly string appInsightsInstrumentationKey;

        /// <summary>
        /// Unique identifier of Microsoft Azure Active Directory in which application is installed.
        /// </summary>
        private readonly string tenantId;

        /// <summary>
        /// Task module height.
        /// </summary>
        private readonly int taskModuleHeight = 460;

        /// <summary>
        /// Task module width.
        /// </summary>
        private readonly int taskModuleWidth = 600;

        /// <summary>
        /// Task module height.
        /// </summary>
        private readonly string noTeamTaskModuleHeight = "small";

        /// <summary>
        /// Task module width.
        /// </summary>
        private readonly string noTeamTaskModuleWidth = "small";

        /// <summary>
        /// Instance of badge user helper to create and award badges, view earned badges.
        /// </summary>
        private readonly IBadgrUserHelper badgeUserHelper;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Generating custom JWT token and retrieving Badgr access token for user.
        /// </summary>
        private readonly ITokenHelper tokenHelper;

        /// <summary>
        /// Reads and writes conversation state for your bot to storage.
        /// </summary>
        private readonly BotState conversationState;

        /// <summary>
        /// Stores user specific data.
        /// </summary>
        private readonly BotState userState;

        /// <summary>
        /// Represents a set of key/value application configuration properties for Badges bot.
        /// </summary>
        private readonly BotSettings configurationSettings;

        /// <summary>
        /// Represents a set of key/value application configuration properties for Badges bot.
        /// </summary>
        private readonly BadgeApiAppSettings badgeApiAppSettings;

        /// <summary>
        /// Open badges bot adapter.
        /// </summary>
        private readonly BotFrameworkAdapter botAdapter;

        /// <summary>
        /// Helper to get issuer group entity ID.
        /// </summary>
        private readonly IBadgrIssuerHelper badgrIssuerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeBot"/> class.
        /// </summary>
        /// <param name="conversationState">Reads and writes conversation state for your bot to storage.</param>
        /// <param name="userState">Reads and writes user specific data to storage.</param>
        /// <param name="tokenHelper">Generating custom JWT token and retrieving Badgr access token for user.</param>
        /// <param name="badgeUserHelper">Instance of badge user helper.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for Badges bot.</param>
        /// <param name="badgeApiAppSettings">Represents a set of key/value application configuration properties for Badges bot.</param>
        /// <param name="oAuthSettings">Represents a set of key/value application configuration properties for OAuth connection.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="botAdapter">Open badges bot adapter.</param>
        /// <param name="badgrIssuerHelper">Helper to get issuer group entity ID.</param>
        public BadgeBot(
            ConversationState conversationState,
            UserState userState,
            ITokenHelper tokenHelper,
            IBadgrUserHelper badgeUserHelper,
            IOptionsMonitor<BotSettings> optionsAccessor,
            IOptionsMonitor<BadgeApiAppSettings> badgeApiAppSettings,
            IOptionsMonitor<OAuthSettings> oAuthSettings,
            ILogger<BadgeBot> logger,
            BotFrameworkAdapter botAdapter,
            IBadgrIssuerHelper badgrIssuerHelper)
        {
            this.configurationSettings = optionsAccessor.CurrentValue;
            this.appBaseUrl = this.configurationSettings.AppBaseUri;
            this.connectionName = oAuthSettings.CurrentValue.ConnectionName;
            this.appInsightsInstrumentationKey = this.configurationSettings.AppInsightsInstrumentationKey;
            this.tenantId = this.configurationSettings.TenantId;

            this.badgeApiAppSettings = badgeApiAppSettings.CurrentValue;

            this.logger = logger;
            this.conversationState = conversationState;
            this.userState = userState;
            this.tokenHelper = tokenHelper;
            this.badgeUserHelper = badgeUserHelper;

            this.botAdapter = botAdapter;
            this.badgrIssuerHelper = badgrIssuerHelper;
        }

        /// <summary>
        /// Method will be invoked on each bot turn.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (!this.IsActivityFromExpectedTenant(turnContext))
            {
                this.logger.LogInformation($"Unexpected tenant Id {turnContext.Activity.Conversation.TenantId}", SeverityLevel.Warning);
                await turnContext.SendActivityAsync(activity: MessageFactory.Text(Strings.InvalidTenant));
            }
            else
            {
                // Get the current culture info to use in resource files
                string locale = turnContext.Activity.Entities?.Where(entity => entity.Type == "clientInfo").First().Properties["locale"].ToString();

                if (!string.IsNullOrEmpty(locale))
                {
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
                }

                await base.OnTurnAsync(turnContext, cancellationToken);

                // Save any state changes that might have occurred during the turn.
                await this.conversationState.SaveChangesAsync(turnContext, force: false, cancellationToken);
                await this.userState.SaveChangesAsync(turnContext, force: false, cancellationToken);
            }
        }

        /// <summary>
        /// Overriding to send welcome card once Bot/ME is installed in team.
        /// </summary>
        /// <param name="membersAdded">A list of all the members added to the conversation, as described by the conversation update activity.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Welcome card  when bot is added first time by user.</returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            this.logger.LogInformation($"conversationType: {activity.Conversation?.ConversationType}, MemberCount: {membersAdded?.Count}");

            if (membersAdded.Where(member => member.Id == activity.Recipient.Id).FirstOrDefault() != null)
            {
                this.logger.LogInformation($"Bot added {activity.Conversation.Id}");
                var welcomeCardImageUrl = new Uri(new Uri(this.appBaseUrl), "/images/welcome.png");
                await turnContext.SendActivityAsync(MessageFactory.Attachment(WelcomeCard.GetWelcomeCardAttachment(welcomeCardImageUrl)), cancellationToken);
            }
        }

        /// <summary>
        /// Method overridden to show task module response.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Messaging extension action commands.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                var activity = turnContext.Activity;
                var activityState = ((JObject)activity.Value).GetValue("state")?.ToString();

                // Check for Badgr token.
                var userBadgrToken = await (turnContext.Adapter as IUserTokenProvider).GetUserTokenAsync(turnContext, this.connectionName, activityState, cancellationToken);
                if (userBadgrToken == null)
                {
                    // Token is not present in bot framework. Create sign in link and send sign in card to user.
                    return await this.CreateSignInCardAsync(turnContext, cancellationToken, Strings.SignInButtonText);
                }

                // Validate user's Microsoft Teams email ID against Badgr account email ID.
                var validationResponse = await this.ValidateUserEmailId(turnContext, userBadgrToken);
                if (validationResponse != null)
                {
                    // Non empty response represents email validation failed. Return validation error response.
                    return validationResponse;
                }

                return await this.CreateSignInSuccessResponse(turnContext, action.Context.Theme);
            }
            catch (UnauthorizedAccessException ex)
            {
                // If token for Badgr expires, sign out user from bot and send sign in card.
                this.logger.LogError(ex, ex.Message);
                return await this.SignOutUserFromBotAsync(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// Method overridden to send card in team after awarding badge.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Messaging extension action commands.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity as Activity;
            var badgeDetails = JsonConvert.DeserializeObject<ViewBadge>(action.Data.ToString());
            var awardRecipients = badgeDetails.AwardRecipients.ToList();
            var awardedBy = badgeDetails.AwardedBy;

            // Get team members in Team.
            var teamsDetails = turnContext.Activity.TeamsGetTeamInfo();
            var channelMembers = await TeamsInfo.GetTeamMembersAsync(turnContext, teamsDetails.Id, cancellationToken);

            for (var recipientCount = 0; recipientCount < badgeDetails.AwardRecipients.Count; recipientCount++)
            {
                var memberEmail = badgeDetails.AwardRecipients[recipientCount];
                badgeDetails.AwardRecipients[recipientCount] = channelMembers.Where(member => member.Email == memberEmail).Select(member => member.Name).FirstOrDefault();
            }

            badgeDetails.AwardedBy = channelMembers.Where(member => member.Email == badgeDetails.AwardedBy).Select(member => member.Name).FirstOrDefault();
            var sentActivity = await turnContext.SendActivityAsync(MessageFactory.Attachment(AwardCard.GetAwardBadgeAttachment(badgeDetails)));

            // Get activity for mentioning members who are awarded with badge.
            var mentionActivity = await this.GetMentionActivityAsync(awardRecipients, awardedBy, turnContext, cancellationToken);

            if (mentionActivity != null)
            {
                if (badgeDetails.CommandContext.Equals("compose"))
                {
                    // Send mentions as reply to card sent above.
                    var conversationReference = activity.GetReplyConversationReference(sentActivity);
                    conversationReference.Conversation.Id = conversationReference.Conversation.Id + ";messageid=" + sentActivity.Id;
                    mentionActivity.ApplyConversationReference(conversationReference);
                    await turnContext.SendActivityAsync(mentionActivity);
                }
                else
                {
                    await turnContext.SendActivityAsync(mentionActivity);
                }
            }

            return default;
        }

        private async Task<MessagingExtensionActionResponse> ValidateUserEmailId(ITurnContext<IInvokeActivity> turnContext, TokenResponse userBadgrToken)
        {
            var activity = turnContext.Activity;

            // Get team members in Team.
            var teamsDetails = activity.TeamsGetTeamInfo();
            if (teamsDetails?.Id == null)
            {
                this.logger.LogError("Team ID is empty for user " + activity.From.AadObjectId);
                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Card = TeamNotFoundCard.GetAttachment(),
                            Height = this.noTeamTaskModuleHeight,
                            Width = this.noTeamTaskModuleWidth,
                        },
                    },
                };
            }

            var channelMembers = await TeamsInfo.GetTeamMembersAsync(turnContext, teamsDetails.Id);

            // Get user email ID.
            var userTeamsEmailId = channelMembers.First(member => member.AadObjectId == activity.From.AadObjectId).Email;

            // Validate if Teams email ID matches with Badgr account email ID.
            var isUserEmailIdValid = await this.badgeUserHelper.ValidateUserEmailIdAsync(userTeamsEmailId, userBadgrToken.Token);
            if (!isUserEmailIdValid)
            {
                // If the user has not signed in with the same Azure Active Directory account which was used to authenticate to Teams,
                // then revoke all access tokens generated for user so that user can sign in again with the same Azure Active Directory account which was used to authenticate to Teams.

                // Get all access tokens generated for user.
                var badgeAccessTokens = await this.badgeUserHelper.GetUserAccessTokenAsync(userBadgrToken.Token);
                if (badgeAccessTokens != null && badgeAccessTokens.Any())
                {
                    try
                    {
                        // Revoke all access tokens generated for user.
                        bool isAccessTokenRevokeSuccess = await this.badgeUserHelper.RevokeUserAccessTokenAsync(userBadgrToken.Token, badgeAccessTokens);
                        if (isAccessTokenRevokeSuccess)
                        {
                            this.logger.LogInformation("User has been logged out from Badgr portal.");
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Error while revoking access: {ex.Message}");
                    }
                }
                else
                {
                    this.logger.LogError("Error in retrieving user access token for Badgr API.");
                }

                // Sign out user from bot even if Badgr tokens are retrieved/revoked or not. Otherwise user will not be able to go further and will get stuck in token error.
                return await this.SignOutUserFromBotAsync(turnContext, CancellationToken.None);
            }

            return null;
        }

        /// <summary>
        /// Methods mentions user in respective channel of which they are part after grouping.
        /// </summary>
        /// <param name="awardedToEmails">List of email ID to whom badge is awarded.</param>
        /// <param name="awardedByEmail">Email ID of member who awarded badge.</param>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that sends notification in newly created channel and mention its members.</returns>
        private async Task<Activity> GetMentionActivityAsync(List<string> awardedToEmails, string awardedByEmail, ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var mentionText = new StringBuilder();
                var entities = new List<Entity>();
                var mentions = new List<Mention>();
                var teamsDetails = turnContext.Activity.TeamsGetTeamInfo();
                var channelMembers = await TeamsInfo.GetTeamMembersAsync(turnContext, teamsDetails.Id, cancellationToken);

                var awardedToMemberDetails = channelMembers.Where(member => awardedToEmails.Contains(member.Email)).Select(member => new ChannelAccount { Id = member.Id, Name = member.Name });
                var awardedByMemberDetails = channelMembers.Where(member => member.Email == awardedByEmail).Select(member => new ChannelAccount { Id = member.Id, Name = member.Name }).FirstOrDefault();

                foreach (var member in awardedToMemberDetails)
                {
                    var mention = new Mention
                    {
                        Mentioned = new ChannelAccount()
                        {
                            Id = member.Id,
                            Name = member.Name,
                        },
                        Text = $"<at>{XmlConvert.EncodeName(member.Name)}</at>",
                    };
                    mentions.Add(mention);
                    entities.Add(mention);
                    mentionText.Append(mention.Text).Append(',');
                }

                var awardedBymention = new Mention
                {
                    Mentioned = new ChannelAccount()
                    {
                        Id = awardedByMemberDetails.Id,
                        Name = awardedByMemberDetails.Name,
                    },
                    Text = $"<at>{XmlConvert.EncodeName(awardedByMemberDetails.Name)}</at>",
                };
                entities.Add(awardedBymention);

                var notificationActivity = MessageFactory.Text(string.Format(Strings.MentionText, mentionText.ToString(), awardedBymention.Text));
                notificationActivity.Entities = entities;
                return notificationActivity;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error while mentioning channel member in respective channels.");
                return null;
            }
        }

        /// <summary>
        /// Verify if the tenant id in the message is the same tenant id used when application was configured.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <returns>A boolean, true if tenant provided is expected tenant.</returns>
        private bool IsActivityFromExpectedTenant(ITurnContext turnContext)
        {
            return turnContext.Activity.Conversation.TenantId.Equals(this.tenantId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Method creates sign in card response.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="signInText">Text to be displayed on sign in card.</param>
        /// <returns>Returns sign in card response.</returns>
        private async Task<MessagingExtensionActionResponse> CreateSignInCardAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken, string signInText)
        {
            var signInLink = await this.botAdapter.GetOauthSignInLinkAsync(turnContext, this.connectionName, cancellationToken);

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = MessagingExtensionAuthType,
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                            {
                                new CardAction
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Value = signInLink,
                                    Title = signInText,
                                },
                            },
                    },
                },
            };
        }

        /// <summary>
        /// Method to sign out user from bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns sign in card in response after logging out user from bot.</returns>
        private async Task<MessagingExtensionActionResponse> SignOutUserFromBotAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await this.botAdapter.SignOutUserAsync(turnContext, this.connectionName, userId: null, cancellationToken);
            return await this.CreateSignInCardAsync(turnContext, cancellationToken, Strings.InvalidAccountText);
        }

        /// <summary>
        /// Creates response after user is validated after successful sign in.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="theme">Microsoft Teams theme name set by user.</param>
        /// <returns>Returns task module response where user is redirected after successful sign in.</returns>
        private async Task<MessagingExtensionActionResponse> CreateSignInSuccessResponse(ITurnContext<IInvokeActivity> turnContext, string theme)
        {
            // Get context from where task module is invoked.
            var commandContext = ((JObject)turnContext.Activity.Value).GetValue("commandContext")?.ToString();

            // Generate custom JWT token to authenticate in app API controller.
            var customAPIAuthenticationToken = this.tokenHelper.GenerateInternalAPIToken(turnContext.Activity.ServiceUrl, turnContext.Activity.From.Id, jwtExpiryMinutes: 60);

            // Check for Badgr token.
            var userBadgrToken = await (turnContext.Adapter as IUserTokenProvider).GetUserTokenAsync(turnContext, this.connectionName, null, CancellationToken.None);
            if (userBadgrToken == null)
            {
                // Token is not present in bot framework. Create sign in link and send sign in card to user.
                return await this.CreateSignInCardAsync(turnContext, CancellationToken.None, Strings.SignInButtonText);
            }

            var entitiyId = await this.badgrIssuerHelper.GetIssuerEntityId();
            return new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = $"{this.appBaseUrl}/AllBadges?token={customAPIAuthenticationToken}&telemetry={this.appInsightsInstrumentationKey}&entityId={entitiyId}&theme={theme}&badgrUrl={this.badgeApiAppSettings.BaseUrl.Replace("api.", string.Empty)}&commandContext={commandContext}",
                        Height = this.taskModuleHeight,
                        Width = this.taskModuleWidth,
                        Title = Strings.TaskModuleTitle,
                    },
                },
            };
        }
    }
}
