// <copyright file="ServicesExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Teams.App.Badges.Helpers;
    using Microsoft.Teams.App.Badges.Helpers.DelegatingHandlers;
    using Microsoft.Teams.App.Badges.Models;
    using Polly;
    using Polly.Extensions.Http;

    /// <summary>
    /// Class to extend ServiceCollection .
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds application configuration settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OAuthSettings>(options => configuration.GetSection("OAuth").Bind(options));
            services.Configure<TokenSettings>(options => configuration.GetSection("Token").Bind(options));
            services.Configure<StorageSettings>(options => configuration.GetSection("Storage").Bind(options));
            services.Configure<BadgeApiAppSettings>(options => configuration.GetSection("BadgeAPI").Bind(options));
            services.Configure<BotSettings>(options => configuration.GetSection("Bot").Bind(options));
            services.Configure<SuperUserSettings>(options => configuration.GetSection("KeyVault").Bind(options));
        }

        /// <summary>
        /// Adds HTTP clients for API helpers to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void AddHTTPClients(this IServiceCollection services)
        {
            services.AddHttpClient<IBadgrApiHelper, BadgrApiHelper>().AddPolicyHandler(GetRetryPolicy());
            services.AddTransient<IssuerHttpHandler>();
            services.AddTransient<UserHttpHandler>();
            services.AddHttpClient<IBadgrUserHelper, BadgrUserHelper>().AddHttpMessageHandler<UserHttpHandler>().AddPolicyHandler(GetRetryPolicy());
            services.AddHttpClient<IBadgrIssuerHelper, BadgrIssuerHelper>().AddHttpMessageHandler<IssuerHttpHandler>().AddPolicyHandler(GetRetryPolicy());
        }

        /// <summary>
        /// Adds helpers to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void AddHelpers(this IServiceCollection services)
        {
            services.AddTransient<IKeyVaultHelper, KeyVaultHelper>();
            services.AddSingleton<ITokenHelper, TokenHelper>();
            services.AddSingleton<IKeyVaultClient>(new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)));
        }

        /// <summary>
        /// Adds custom JWT authentication to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddCustomJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateAudience = true,
                       ValidAudiences = new List<string> { configuration.GetSection("Bot")["AppBaseUri"] },
                       ValidIssuers = new List<string> { configuration.GetSection("Bot")["AppBaseUri"] },
                       ValidateIssuer = true,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("Token")["SecurityKey"])),
                       RequireExpirationTime = true,
                       ValidateLifetime = true,
                       ClockSkew = TimeSpan.FromSeconds(30),
                   };
               });
        }

        /// <summary>
        /// Adds user state and conversation state to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void AddBotStates(this IServiceCollection services)
        {
            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Storage for conversation state and user state.
            services.AddSingleton<IStorage>(new MemoryStorage());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.IsSuccessStatusCode == false)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
