﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that short-circuits Stripe test events.
    /// </summary>
    /// <remarks>Somewhat similar to the <see cref="WebHookPingResponseFilter"/>.</remarks>
    public class StripeTestEventResponseFilter : IResourceFilter, IWebHookReceiver
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="StripeTestEventResponseFilter"/> instance.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public StripeTestEventResponseFilter(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<StripeTestEventResponseFilter>();
        }

        /// <inheritdoc />
        public string ReceiverName => StripeConstants.ReceiverName;

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all
        /// <see cref="StripeTestEventResponseFilter"/> instances. This filter should execute after all other
        /// applicable built-in WebHook filters.
        /// </summary>
        public static int Order => WebHookPingResponseFilter.Order;

        /// <inheritdoc />
        public bool IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (!routeData.TryGetWebHookReceiverName(out var receiverName) ||
                !IsApplicable(receiverName) ||
                _configuration.IsTrue(StripeConstants.PassThroughTestEventsConfigurationKey))
            {
                return;
            }

            var notificationId = (string)routeData.Values[StripeConstants.NotificationIdKeyName];
            if (StripeVerifyNotificationIdFilter.IsTestEvent(notificationId))
            {
                // Short-circuit this test event.
                _logger.LogInformation(1, "Ignoring a Stripe Test Event.");
                context.Result = new OkResult();
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }
    }
}
