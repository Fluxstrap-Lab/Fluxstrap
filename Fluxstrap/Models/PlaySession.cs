namespace Fluxstrap.Models
{
    public class PlaySession
    {
        public long PlaceId { get; set; }
        public string GameName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }
}
