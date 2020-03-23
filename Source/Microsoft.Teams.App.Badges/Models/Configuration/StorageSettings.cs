// <copyright file="StorageSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.App.Badges.Models
{
    /// <summary>
    /// Provides app setting related to Azure Table Storage.
    /// </summary>
    public class StorageSettings
    {
        /// <summary>
        /// Gets or sets Azure Table Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
