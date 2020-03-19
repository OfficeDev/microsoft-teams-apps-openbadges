// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">String array of arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates instance of web host builder.
        /// </summary>
        /// <param name="args">Array of arguments.</param>
        /// <returns>Web host builder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
             .ConfigureLogging((hostingContext, logging) =>
             {
                 // hostingContext.HostingEnvironment can be used to determine environments as well.
                 var appInsightKey = hostingContext.Configuration.GetSection("Bot")["AppInsightsInstrumentationKey"];
                 logging.AddApplicationInsights(appInsightKey);
             });
    }
}
