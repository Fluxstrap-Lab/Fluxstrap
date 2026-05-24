using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        public BehaviourViewModel()
        {
            RefreshCrashDumps();
        }

        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }

        public bool OptimizeRoblox
        {
            get => App.Settings.Prop.OptimizeRoblox;
            set
            {
                App.Settings.Prop.OptimizeRoblox = value;
                OnPropertyChanged(nameof(OptimizeRoblox));
            }
        }

        public int MaxCpuCores
        {
            get => App.Settings.Prop.MaxCpuCores;
            set
            {
                if (value < 0) value = 0;
                if (value > Environment.ProcessorCount) value = Environment.ProcessorCount;
                App.Settings.Prop.MaxCpuCores = value;
                OnPropertyChanged(nameof(MaxCpuCores));
            }
        }

        public bool MemoryCleaner
        {
            get => App.Settings.Prop.MemoryCleaner;
            set
            {
                App.Settings.Prop.MemoryCleaner = value;
                OnPropertyChanged(nameof(MemoryCleaner));
            }
        }

        public int MaxCores => Environment.ProcessorCount;

        public bool UnlockFPS
        {
            get => App.Settings.Prop.UnlockFPS;
            set
            {
                App.Settings.Prop.UnlockFPS = value;
                OnPropertyChanged(nameof(UnlockFPS));
            }
        }

        public int FPSCap
        {
            get => App.Settings.Prop.FPSCap;
            set
            {
                if (value < 30) value = 30;
                if (value > 999) value = 999;
                App.Settings.Prop.FPSCap = value;
                OnPropertyChanged(nameof(FPSCap));
            }
        }

        public bool ForceGraphicsQuality
        {
            get => App.Settings.Prop.ForceGraphicsQuality;
            set
            {
                App.Settings.Prop.ForceGraphicsQuality = value;
                OnPropertyChanged(nameof(ForceGraphicsQuality));
            }
        }

        public int GraphicsQualityLevel
        {
            get => App.Settings.Prop.GraphicsQualityLevel;
            set
            {
                if (value < 1) value = 1;
                if (value > 21) value = 21;
                App.Settings.Prop.GraphicsQualityLevel = value;
                OnPropertyChanged(nameof(GraphicsQualityLevel));
            }
        }

        // HTTP Cache Auto-Cleaner

        public bool AutoCleanRobloxCache
        {
            get => App.Settings.Prop.AutoCleanRobloxCache;
            set
            {
                App.Settings.Prop.AutoCleanRobloxCache = value;
                OnPropertyChanged(nameof(AutoCleanRobloxCache));
            }
        }

        public string RobloxCacheSize
        {
            get
            {
                string cacheDir = Paths.RobloxHttpCache;
                if (!Directory.Exists(cacheDir))
                    return "0 B";

                try
                {
                    long size = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories)
                        .Sum(file => new FileInfo(file).Length);
                    return FormatSize(size);
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public ICommand CleanRobloxCacheCommand => new RelayCommand(() =>
        {
            string cacheDir = Paths.RobloxHttpCache;
            if (!Directory.Exists(cacheDir))
                return;

            try
            {
                foreach (string file in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
                {
                    try { File.Delete(file); }
                    catch { }
                }

                foreach (string dir in Directory.GetDirectories(cacheDir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                            Directory.Delete(dir, false);
                    }
                    catch { }
                }

                OnPropertyChanged(nameof(RobloxCacheSize));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BehaviourViewModel::CleanRobloxCache", ex);
            }
        });

        // Crash Report Collector

        public ObservableCollection<CrashDumpEntry> CrashDumps { get; } = new();

        public ICommand RefreshCrashDumpsCommand => new RelayCommand(RefreshCrashDumps);

        public ICommand DeleteCrashDumpCommand => new RelayCommand<CrashDumpEntry>(entry =>
        {
            if (entry is null) return;
            try
            {
                File.Delete(entry.FilePath);
                CrashDumps.Remove(entry);
                OnPropertyChanged(nameof(HasCrashDumps));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BehaviourViewModel::DeleteCrashDump", ex);
            }
        });

        public ICommand OpenCrashDumpFolderCommand => new RelayCommand<CrashDumpEntry>(entry =>
        {
            if (entry is null) return;
            Utilities.OpenFolderInExplorer(entry.FilePath);
        });

        public ICommand OpenAllCrashDumpsFolderCommand => new RelayCommand(() =>
        {
            if (Directory.Exists(Paths.RobloxCrashLogs))
                Utilities.OpenFolderInExplorer(Paths.RobloxCrashLogs);
        });

        public bool HasCrashDumps => CrashDumps.Count > 0;

        private void RefreshCrashDumps()
        {
            CrashDumps.Clear();

            string crashDir = Paths.RobloxCrashLogs;
            if (!Directory.Exists(crashDir))
                return;

            try
            {
                foreach (string file in Directory.GetFiles(crashDir).OrderByDescending(f => File.GetLastWriteTime(f)).Take(20))
                {
                    var info = new FileInfo(file);
                    CrashDumps.Add(new CrashDumpEntry
                    {
                        FilePath = file,
                        FileName = info.Name,
                        FileSize = FormatSize(info.Length),
                        CreatedAt = info.LastWriteTime
                    });
                }

                OnPropertyChanged(nameof(HasCrashDumps));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BehaviourViewModel::RefreshCrashDumps", ex);
            }
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        // Quick Launch Favorites

        public ObservableCollection<FavoritePlace> FavoritePlaces => App.Settings.Prop.FavoritePlaces;

        public string? NewFavoritePlaceId { get; set; }
        public string? NewFavoritePlaceName { get; set; }

        public ICommand AddFavoritePlaceCommand => new RelayCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(NewFavoritePlaceId) || string.IsNullOrWhiteSpace(NewFavoritePlaceName))
                return;

            if (!long.TryParse(NewFavoritePlaceId.Trim(), out long placeId))
                return;

            if (FavoritePlaces.Any(x => x.PlaceId == placeId))
                return;

            FavoritePlaces.Add(new FavoritePlace
            {
                PlaceId = placeId,
                Name = NewFavoritePlaceName.Trim()
            });

            NewFavoritePlaceId = "";
            NewFavoritePlaceName = "";
            OnPropertyChanged(nameof(NewFavoritePlaceId));
            OnPropertyChanged(nameof(NewFavoritePlaceName));
            OnPropertyChanged(nameof(FavoritePlaces));
        });

        public ICommand RemoveFavoritePlaceCommand => new RelayCommand<FavoritePlace>(place =>
        {
            if (place is null) return;
            FavoritePlaces.Remove(place);
            OnPropertyChanged(nameof(FavoritePlaces));
        });

        public ICommand LaunchFavoritePlaceCommand => new RelayCommand<FavoritePlace>(place =>
        {
            if (place is null) return;
            try
            {
                string uri = $"roblox-player:1+launchmode:play+placeId:{place.PlaceId}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BehaviourViewModel::LaunchFavoritePlace", ex);
            }
        });

        // Auto-Relaunch on Crash

        public bool AutoRelaunchOnCrash
        {
            get => App.Settings.Prop.AutoRelaunchOnCrash;
            set
            {
                App.Settings.Prop.AutoRelaunchOnCrash = value;
                OnPropertyChanged(nameof(AutoRelaunchOnCrash));
            }
        }

        // Custom Launch Arguments

        public string CustomLaunchArgs
        {
            get => App.Settings.Prop.CustomLaunchArgs;
            set
            {
                App.Settings.Prop.CustomLaunchArgs = value;
                OnPropertyChanged(nameof(CustomLaunchArgs));
            }
        }

        // Resource Monitor

        public bool ShowResourceMonitor
        {
            get => App.Settings.Prop.ShowResourceMonitor;
            set
            {
                App.Settings.Prop.ShowResourceMonitor = value;
                OnPropertyChanged(nameof(ShowResourceMonitor));
            }
        }

        // Anti-AFK

        public bool AntiAFK
        {
            get => App.Settings.Prop.AntiAFK;
            set
            {
                App.Settings.Prop.AntiAFK = value;
                OnPropertyChanged(nameof(AntiAFK));
            }
        }

        // Auto-Rejoin on Disconnect

        public bool AutoRejoinOnDisconnect
        {
            get => App.Settings.Prop.AutoRejoinOnDisconnect;
            set
            {
                App.Settings.Prop.AutoRejoinOnDisconnect = value;
                OnPropertyChanged(nameof(AutoRejoinOnDisconnect));
            }
        }

        // Per-Game Profiles

        public ObservableCollection<GameProfile> GameProfiles => App.Settings.Prop.GameProfiles;

        public string? NewGameProfileUniverseId { get; set; }
        public string? NewGameProfileName { get; set; }
        public int NewGameProfileFpsCap { get; set; } = 240;
        public int NewGameProfileQualityLevel { get; set; } = 10;

        public ICommand AddGameProfileCommand => new RelayCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(NewGameProfileUniverseId) || string.IsNullOrWhiteSpace(NewGameProfileName))
                return;

            if (!long.TryParse(NewGameProfileUniverseId.Trim(), out long universeId))
                return;

            if (GameProfiles.Any(x => x.UniverseId == universeId))
                return;

            GameProfiles.Add(new GameProfile
            {
                UniverseId = universeId,
                GameName = NewGameProfileName.Trim(),
                FpsCap = NewGameProfileFpsCap,
                GraphicsQualityLevel = NewGameProfileQualityLevel,
                UseCustomFpsCap = true,
                UseCustomGraphicsQuality = true
            });

            NewGameProfileUniverseId = "";
            NewGameProfileName = "";
            OnPropertyChanged(nameof(NewGameProfileUniverseId));
            OnPropertyChanged(nameof(NewGameProfileName));
            OnPropertyChanged(nameof(GameProfiles));
        });

        public ICommand RemoveGameProfileCommand => new RelayCommand<GameProfile>(profile =>
        {
            if (profile is null) return;
            GameProfiles.Remove(profile);
            OnPropertyChanged(nameof(GameProfiles));
        });

        // Launch on Startup

        public bool LaunchOnStartup
        {
            get => App.Settings.Prop.LaunchOnStartup;
            set
            {
                App.Settings.Prop.LaunchOnStartup = value;
                Utilities.SetStartupLaunch(value);
                OnPropertyChanged(nameof(LaunchOnStartup));
            }
        }

        // Global Hotkeys

        public bool EnableGlobalHotkeys
        {
            get => App.Settings.Prop.EnableGlobalHotkeys;
            set
            {
                App.Settings.Prop.EnableGlobalHotkeys = value;
                OnPropertyChanged(nameof(EnableGlobalHotkeys));
            }
        }

        // Do Not Disturb

        public bool DNDDuringGameplay
        {
            get => App.Settings.Prop.DNDDuringGameplay;
            set
            {
                App.Settings.Prop.DNDDuringGameplay = value;
                OnPropertyChanged(nameof(DNDDuringGameplay));
            }
        }

        // Session Play Time Tracking

        public bool TrackPlayTime
        {
            get => App.Settings.Prop.TrackPlayTime;
            set
            {
                App.Settings.Prop.TrackPlayTime = value;
                OnPropertyChanged(nameof(TrackPlayTime));
                OnPropertyChanged(nameof(ShowPlayTimeStats));
            }
        }

        public bool ShowPlayTimeStats => App.Settings.Prop.TrackPlayTime;

        public string TodayPlayTime => PlayTimeTracker.FormatTimeSpan(PlayTimeTracker.GetTotalPlayTimeToday());

        public string AllTimePlayTime => PlayTimeTracker.FormatTimeSpan(PlayTimeTracker.GetTotalPlayTimeAllTime());

        public string? CurrentSessionTime
        {
            get
            {
                if (PlayTimeTracker.CurrentSession is null)
                    return null;
                return PlayTimeTracker.FormatTimeSpan(DateTime.Now - PlayTimeTracker.CurrentSession.StartTime);
            }
        }

        public string SessionCount => PlayTimeTracker.Sessions.Count.ToString();

        // Auto-Repair on Crash Loop

        public bool AutoRepairOnCrashLoop
        {
            get => App.Settings.Prop.AutoRepairOnCrashLoop;
            set
            {
                App.Settings.Prop.AutoRepairOnCrashLoop = value;
                OnPropertyChanged(nameof(AutoRepairOnCrashLoop));
            }
        }

        // Streaming Mode

        public bool StreamingMode
        {
            get => App.Settings.Prop.StreamingMode;
            set
            {
                App.Settings.Prop.StreamingMode = value;
                OnPropertyChanged(nameof(StreamingMode));
            }
        }
    }

    public class CrashDumpEntry
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileSize { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
