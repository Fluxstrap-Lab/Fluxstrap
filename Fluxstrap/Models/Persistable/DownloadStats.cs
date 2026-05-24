namespace Fluxstrap.Models.Persistable
{
    public class DownloadStats
    {
        public long TotalBytesDownloaded { get; set; }

        public int TotalPackagesDownloaded { get; set; }

        public int TotalDownloadsCompleted { get; set; }

        public int TotalFailedDownloads { get; set; }

        public DateTime LastDownloadTime { get; set; } = DateTime.MinValue;
    }
}
