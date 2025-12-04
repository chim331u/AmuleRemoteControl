namespace AmuleRemoteControl.Components.Interfaces
{
    public interface INetworkHelper
    {
        Task<string> SendRequest(string page);
        Task<string> SendRequest();
        Task<string> SendRequest(string page, Dictionary<string, string> parameters);
        Task<string> PostRequest(string page, Dictionary<string, string> parameters);
    }
}
