using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    public interface IAmuleRemoteServices
    {
        Task<List<Servers>> GetServers();
        Task<List<Servers>> ConnectServer(string serverId, string serverPort);
        Task<List<Servers>> RemoveServer(string serverId, string serverPort);

        Task<List<DownloadFile>> GetDownloading();
        Task<List<DownloadFile>> PostDownloadCommand(string fileId, string command);
        Task<List<DownloadFile>> PostDownloadCommand(List<string> filesId, string command);

        Task<List<UploadFile>> GetUploads();

        Task<List<DownloadFile>> AddEd2kLink(string ed2kLink);

        Task<Stats> GetStats();

        Task<string> GetaMuleLog();
        Task<string> GetServerInfo();

        Task<List<Search>> SearchFiles(string searchText, string searchType, string? targetCat);
        Task<List<Search>> RefreshSearch();
        Task<string> DownloadSearch(string fileId);

        Task<PreferenceModel> GetPreferences();
        Task<PreferenceModel> PostPreferencesCommand(PreferenceModel preference);

        Stats? Status { get; set; }
        event EventHandler? StatusChanged;
        Task AutoStatus();


        string? DownSpeed { get; set; }
        event EventHandler? DownChanged;
        Task AutoDownSpeed();

    }
}
