﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Globalization;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Storage;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Defines a <see cref="DbContext"/> which contains the set of WebHook <see cref="Registration"/> instances.
    /// </summary>
    public class WebHookStoreContext : DbContext
    {
        private readonly string _tableName;
        private readonly string _schemaName = "WebHooks";
        internal static string ConnectionStringName = "MS_SqlStoreConnectionString";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStoreContext"/> class.
        /// </summary>
        public WebHookStoreContext() : base("name=" + ConnectionStringName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStoreContext"/> class using the given string
        /// as the name or connection string for the database to which a connection will be made.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public WebHookStoreContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            ConnectionStringName = nameOrConnectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStoreContext"/> class using the given parameters
        /// as the name or connection string for the database to which a connection will be made.
        /// And configures the database schema name and table name.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        /// <param name="schemaName">Either the schema name.</param>
        /// <param name="tableName">Either the table name.</param>
        public WebHookStoreContext(string nameOrConnectionString, string schemaName, string tableName)
            : this(nameOrConnectionString)
        {
            if (string.IsNullOrEmpty(schemaName))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_EmptyString, nameof(schemaName));
                throw new ArgumentException(msg);
            }

            if (string.IsNullOrEmpty(tableName))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_EmptyString, nameof(tableName));
                throw new ArgumentException(msg);
            }

            _schemaName = schemaName;
            _tableName = tableName;
        }

        /// <summary>
        /// Gets or sets the current collection of <see cref="Registration"/> instances.
        /// </summary>
        public virtual DbSet<Registration> Registrations { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.HasDefaultSchema(_schemaName);

            if (!string.IsNullOrEmpty(_tableName))
            {
                EntityTypeConfiguration<Registration> registrationConfiguration = modelBuilder.Entity<Registration>();
                registrationConfiguration.ToTable(_tableName);
            }
        }
    }
}
