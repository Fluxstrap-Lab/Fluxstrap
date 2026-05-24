using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class GameStatEntry
    {
        public string GameName { get; set; } = "";
        public long PlaceId { get; set; }
        public int SessionCount { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        public string TotalPlayTimeFormatted => FormatPlayTime(TotalPlayTime);
        public string LastPlayed { get; set; } = "";

        private static string FormatPlayTime(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }
    }

    public class StatisticsViewModel : NotifyPropertyChangedViewModel
    {
        private string _totalPlayTimeToday = "0m 0s";
        private string _totalPlayTimeAllTime = "0m 0s";
        private int _totalSessions;
        private string _averageSessionTime = "0m 0s";
        private string _mostPlayedGame = "-";
        private bool _isLoading = true;

        public string TotalPlayTimeToday
        {
            get => _totalPlayTimeToday;
            set { _totalPlayTimeToday = value; OnPropertyChanged(nameof(TotalPlayTimeToday)); }
        }

        public string TotalPlayTimeAllTime
        {
            get => _totalPlayTimeAllTime;
            set { _totalPlayTimeAllTime = value; OnPropertyChanged(nameof(TotalPlayTimeAllTime)); }
        }

        public int TotalSessions
        {
            get => _totalSessions;
            set { _totalSessions = value; OnPropertyChanged(nameof(TotalSessions)); }
        }

        public string AverageSessionTime
        {
            get => _averageSessionTime;
            set { _averageSessionTime = value; OnPropertyChanged(nameof(AverageSessionTime)); }
        }

        public string MostPlayedGame
        {
            get => _mostPlayedGame;
            set { _mostPlayedGame = value; OnPropertyChanged(nameof(MostPlayedGame)); }
        }

        public ObservableCollection<GameStatEntry> TopGames { get; } = new();

        public bool HasSessions => TotalSessions > 0;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); OnPropertyChanged(nameof(ShowEmptyState)); }
        }
        public bool ShowEmptyState => !IsLoading && !HasSessions;

        public ICommand RefreshCommand => new RelayCommand(LoadStatistics);

        public StatisticsViewModel()
        {
            LoadStatistics();
        }

        public void LoadStatistics()
        {
            IsLoading = true;

            var sessions = PlayTimeTracker.Sessions;

            TotalPlayTimeToday = PlayTimeTracker.FormatTimeSpan(PlayTimeTracker.GetTotalPlayTimeToday());
            TotalPlayTimeAllTime = PlayTimeTracker.FormatTimeSpan(PlayTimeTracker.GetTotalPlayTimeAllTime());
            TotalSessions = sessions.Count;

            if (sessions.Count > 0)
            {
                double avgMinutes = sessions.Average(s => s.Duration.TotalMinutes);
                AverageSessionTime = avgMinutes >= 60
                    ? $"{(int)avgMinutes / 60}h {(int)avgMinutes % 60}m"
                    : $"{(int)avgMinutes}m {(int)(avgMinutes * 60) % 60}s";

                var topGames = sessions
                    .GroupBy(s => s.PlaceId)
                    .Select(g => new GameStatEntry
                    {
                        GameName = g.First().GameName,
                        PlaceId = g.Key,
                        SessionCount = g.Count(),
                        TotalPlayTime = g.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration),
                        LastPlayed = g.Max(s => s.StartTime).ToString("yyyy-MM-dd")
                    })
                    .OrderByDescending(g => g.TotalPlayTime)
                    .Take(10)
                    .ToList();

                TopGames.Clear();
                foreach (var game in topGames)
                    TopGames.Add(game);

                MostPlayedGame = topGames.FirstOrDefault()?.GameName ?? "-";
            }
            else
            {
                LoadFromServerHistory();
            }

            OnPropertyChanged(nameof(HasSessions));
            OnPropertyChanged(nameof(TopGames));
            IsLoading = false;
        }

        private void LoadFromServerHistory()
        {
            var serverHistory = ReadServerHistory();
            if (serverHistory.Count == 0)
                return;

            var withDuration = serverHistory
                .Where(s => s.TimeLeft.HasValue)
                .ToList();

            TimeSpan totalTime = withDuration
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + (s.TimeLeft!.Value - s.TimeJoined));

            var todaySessions = withDuration
                .Where(s => s.TimeJoined.Date == DateTime.Today);
            TimeSpan todayTime = todaySessions
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + (s.TimeLeft!.Value - s.TimeJoined));

            TotalPlayTimeToday = FormatTimeSpan(todayTime);
            TotalPlayTimeAllTime = FormatTimeSpan(totalTime);
            TotalSessions = serverHistory.Count;

            if (withDuration.Count > 0)
            {
                double avgMinutes = withDuration.Average(s => (s.TimeLeft!.Value - s.TimeJoined).TotalMinutes);
                AverageSessionTime = avgMinutes >= 60
                    ? $"{(int)avgMinutes / 60}h {(int)avgMinutes % 60}m"
                    : $"{(int)avgMinutes}m {(int)(avgMinutes * 60) % 60}s";
            }

            var universeNames = new Dictionary<long, string>();
            foreach (var entry in serverHistory)
            {
                string name = entry.UniverseDetails?.Data?.Name ?? "";
                if (!string.IsNullOrEmpty(name) && entry.UniverseId != 0 && !universeNames.ContainsKey(entry.UniverseId))
                    universeNames[entry.UniverseId] = name;
            }

            var topFromServer = serverHistory
                .GroupBy(s => s.PlaceId)
                .Select(g =>
                {
                    var withDur = g.Where(s => s.TimeLeft.HasValue).ToList();
                    var uni = g.FirstOrDefault(s => s.UniverseId != 0);
                    string name = uni != null && universeNames.TryGetValue(uni.UniverseId, out string? n) && !string.IsNullOrEmpty(n)
                        ? n : $"Place {g.Key}";

                    return new GameStatEntry
                    {
                        GameName = name,
                        PlaceId = g.Key,
                        SessionCount = g.Count(),
                        TotalPlayTime = withDur.Count > 0
                            ? withDur.Aggregate(TimeSpan.Zero, (sum, s) => sum + (s.TimeLeft!.Value - s.TimeJoined))
                            : TimeSpan.Zero,
                        LastPlayed = g.Max(s => s.TimeJoined).ToString("yyyy-MM-dd")
                    };
                })
                .OrderByDescending(g => g.TotalPlayTime)
                .Take(10)
                .ToList();

            TopGames.Clear();
            foreach (var game in topFromServer)
                TopGames.Add(game);

            MostPlayedGame = topFromServer.FirstOrDefault()?.GameName ?? "-";
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        private static List<ActivityData> ReadServerHistory()
        {
            string historyPath = Path.Combine(Paths.Base, "ServerHistory.json");
            if (!File.Exists(historyPath))
                return new();

            try
            {
                var json = File.ReadAllText(historyPath);
                return JsonSerializer.Deserialize<List<ActivityData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
            }
            catch
            {
                return new();
            }
        }
    }
}
