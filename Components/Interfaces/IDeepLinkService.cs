using System;

namespace AmuleRemoteControl.Components.Interfaces
{
    public interface IDeepLinkService
    {
        event EventHandler<DeepLinkEventArgs> LinkReceived;
        void NotifyLinkReceived(string url, DeepLinkSource source);
        DeepLinkEventArgs? GetPendingLink();
    }

    public class DeepLinkEventArgs : EventArgs
    {
        public string Url { get; }
        public DeepLinkSource Source { get; }
        public DateTime ReceivedAt { get; }

        public DeepLinkEventArgs(string url, DeepLinkSource source)
        {
            Url = url;
            Source = source;
            ReceivedAt = DateTime.UtcNow;
        }
    }

    public enum DeepLinkSource
    {
        ExternalApp,
        Browser,
        InAppShare
    }
}
