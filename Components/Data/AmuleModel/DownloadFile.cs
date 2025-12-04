namespace AmuleRemoteControl.Components.Data.AmuleModel
{
    public class DownloadFile
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? Size { get; set; }
        public string? Completed { get; set; }
        public string? DownloadSpeed { get; set; }
        public double Progress { get; set; }
        public string? Sources { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }
}
