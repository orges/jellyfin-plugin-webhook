﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.Webhook.Destinations;
using Jellyfin.Plugin.Webhook.Helpers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Webhook.Notifiers
{
    /// <summary>
    /// Playback start notifier.
    /// </summary>
    public class PlaybackStartNotifier : IEventConsumer<PlaybackStartEventArgs>
    {
        private readonly IApplicationHost _applicationHost;
        private readonly WebhookSender _webhookSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackStartNotifier"/> class.
        /// </summary>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        /// <param name="webhookSender">Instance of the <see cref="WebhookSender"/>.</param>
        public PlaybackStartNotifier(
            IApplicationHost applicationHost,
            WebhookSender webhookSender)
        {
            _applicationHost = applicationHost;
            _webhookSender = webhookSender;
        }

        /// <inheritdoc />
        public async Task OnEvent(PlaybackStartEventArgs eventArgs)
        {
            if (eventArgs.Item == null)
            {
                // No item.
                return;
            }

            if (eventArgs.Item.IsThemeMedia)
            {
                // Don't report theme song or local trailer playback.
                return;
            }

            if (eventArgs.Users.Count == 0)
            {
                // No users in playback session.
                return;
            }

            var dataObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                .AddBaseItemData(_applicationHost, eventArgs.Item)
                .AddPlaybackProgressData(eventArgs);
            dataObject[nameof(NotificationType)] = NotificationType.PlaybackStart;

            foreach (var user in eventArgs.Users)
            {
                var userDataObject = new Dictionary<string, object>(dataObject)
                {
                    ["Username"] = user.Username,
                    ["UserId"] = user.Id
                };

                await _webhookSender.SendItemNotification(NotificationType.PlaybackStart, userDataObject, eventArgs.Item.GetType());
            }
        }
    }
}