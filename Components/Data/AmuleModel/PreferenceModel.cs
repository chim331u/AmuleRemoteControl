namespace AmuleRemoteControl.Components.Data.AmuleModel
{
    public class PreferenceModel
    {
        public string? PageRefreshInterval { get; set; }
        public bool UseGzipCompression { get; set; }
        public string? MaxDownloadRate { get; set; }
        public string? MaxUploadRate { get; set; }
        public string? SlotAllocation { get; set; }
        public string? MaxTotalConnection { get; set; }
        public string? MaxSourcePerFile { get; set; }
        public bool AutoConnectAtStartUp { get; set; }
        public bool ReconnectWhenLostConnection { get; set; }
        public string? TCPport { get; set; }
        public string? UDPport { get; set; }
        public bool DisableUDPconnections { get; set; }
        public string? MaxDownloadRate_Statistic { get; set; }
        public string? MaxUploadRate_Statistic { get; set; }
        public bool CheckFreeSpace { get; set; }
        public bool AddDownloadFileAutoPriority { get; set; }
        public bool NewSharedFileAutoPriority { get; set; }
        public bool ICHactive { get; set; }
        public bool AICHtrustsEveryHash { get; set; }
        public bool AllocFullChunksPartFiles { get; set; }
        public bool AllocFullDiskSpacePartFiles { get; set; }
        public bool AddFileDownloadQueuePauseMode { get; set; }
        public bool ExtractMetadataTags { get; set; }
        public string? MinimumFreeSpaceMb { get; set; }
    }
}
