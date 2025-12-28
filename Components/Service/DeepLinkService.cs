using System;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    public class DeepLinkService : IDeepLinkService
    {
        private readonly ILogger<DeepLinkService> _logger;

        public event EventHandler<DeepLinkEventArgs>? LinkReceived;
        private DeepLinkEventArgs? _pendingLink;

        public DeepLinkService(ILogger<DeepLinkService> logger)
        {
            _logger = logger;
        }

        public void NotifyLinkReceived(string url, DeepLinkSource source)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Received empty deep link URL");
                return;
            }

            _logger.LogInformation($"Deep link received from {source}: {url}");

            // Invoke the event
            try
            {
                var args = new DeepLinkEventArgs(url, source);
                _pendingLink = args;
                LinkReceived?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling deep link event");
            }
        }

        public DeepLinkEventArgs? GetPendingLink()
        {
            var link = _pendingLink;
            _pendingLink = null;
            return link;
        }
    }
}
