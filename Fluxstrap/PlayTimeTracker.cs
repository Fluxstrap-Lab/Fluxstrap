using Fluxstrap.Models;

namespace Fluxstrap
{
    public static class PlayTimeTracker
    {
        private static readonly string FilePath = Path.Combine(Paths.Base, "SessionHistory.json");

        private static List<PlaySession> _sessions = new();

        public static IReadOnlyList<PlaySession> Sessions => _sessions.AsReadOnly();

        public static PlaySession? CurrentSession { get; private set; }

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    _sessions = JsonSerializer.Deserialize<List<PlaySession>>(json) ?? new();
                }
            }
            catch
            {
                _sessions = new();
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                string json = JsonSerializer.Serialize(_sessions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PlayTimeTracker::Save", ex);
            }
        }

        public static void StartSession(long placeId, string gameName)
        {
            CurrentSession = new PlaySession
            {
                PlaceId = placeId,
                GameName = gameName,
                StartTime = DateTime.Now
            };
        }

        public static void EndSession()
        {
            if (CurrentSession is null)
                return;

            CurrentSession.EndTime = DateTime.Now;
            _sessions.Add(CurrentSession);
            CurrentSession = null;

            // keep last 1000 sessions
            if (_sessions.Count > 1000)
                _sessions = _sessions.Skip(_sessions.Count - 1000).ToList();

            Save();
        }

        public static TimeSpan GetTotalPlayTimeToday()
        {
            return _sessions
                .Where(s => s.StartTime.Date == DateTime.Today)
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
        }

        public static TimeSpan GetTotalPlayTimeAllTime()
        {
            return _sessions
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
        }

        public static TimeSpan GetGamePlayTime(long placeId)
        {
            return _sessions
                .Where(s => s.PlaceId == placeId)
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
        }

        public static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            return $"{ts.Minutes}m {ts.Seconds}s";
        }
    }
}
