using Fluxstrap.AppData;
using Fluxstrap.Integrations;
using Fluxstrap.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fluxstrap
{
    public class Watcher : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_F15 = 0x7E;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private DateTime _lastAfkInput = DateTime.MinValue;
        private DateTime _lastDisconnectTime = DateTime.MinValue;
        private int _consecutiveCrashes = 0;
        private readonly InterProcessLock _lock = new("Watcher");

        private readonly WatcherData? _watcherData;
        
        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly DiscordRichPresence? RichPresence;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

            if (String.IsNullOrEmpty(watcherDataArg))
            {
#if DEBUG
                string path = new RobloxPlayerData().ExecutablePath;
                if (!File.Exists(path))
                    throw new ApplicationException("Roblox player is not been installed");

                using var gameClientProcess = Process.Start(path);

                _watcherData = new() { ProcessId = gameClientProcess.Id };
#else
                throw new Exception("Watcher data not specified");
#endif
            }
            else
            {
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));
            }

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new(_watcherData.LogFile);

                ActivityWatcher.OnGameJoin += delegate
                {
                    _consecutiveCrashes = 0;

                    long placeId = ActivityWatcher.Data.PlaceId;
                    string universeName = ActivityWatcher.Data.UniverseDetails?.Data?.Name ?? "";
                    string gameName = !string.IsNullOrEmpty(universeName) ? universeName : placeId > 0 ? $"Place {placeId}" : "Unknown";

                    PlayHistory.Record(placeId, gameName);

                    if (App.Settings.Prop.TrackPlayTime)
                    {
                        PlayTimeTracker.StartSession(placeId, gameName);
                    }
                };

                ActivityWatcher.OnGameLeave += delegate
                {
                    if (App.Settings.Prop.TrackPlayTime)
                        PlayTimeTracker.EndSession();

                    SaveServerHistory();

                    if (App.Settings.Prop.AutoRejoinOnDisconnect && ActivityWatcher.History.Count > 0)
                    {
                        var lastSession = ActivityWatcher.History[0];
                        if (lastSession.PlaceId != 0 && !string.IsNullOrEmpty(lastSession.JobId) && !lastSession.IsTeleport)
                        {
                            string deeplink = $"roblox-player:1+launchmode:play+placeId:{lastSession.PlaceId}&gameInstanceId:{lastSession.JobId}";
                            _ = AutoRejoin(deeplink);
                        }
                    }
                };

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += delegate
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.CloseMainWindow();
                    };
                }

                if (App.Settings.Prop.UseDiscordRichPresence)
                    RichPresence = new(ActivityWatcher);

                if (App.Settings.Prop.EnableServerBlacklist)
                    ActivityWatcher.OnServerBlocked += OnServerBlocked;
            }

            _notifyIcon = new(this);

            // Start overlay manager (runs in watcher process, not bootstrapper)
            if (App.Settings.Prop.OverlayEnabled)
            {
                Task.Run(() => Utility.GameOverlayManager.Start(ActivityWatcher));
            }

            // Apply performance optimizations (GPU overclocker, power plans)
            if (App.Settings.Prop.OverClockCPU || App.Settings.Prop.OverClockGPU)
            {
                Task.Run(() => Utility.AggressivePerformanceManager.EnableHighPerformance());
            }

            // Apply process rename for EuroTrucks2 mode
            if (App.Settings.Prop.RenameClientToEuroTrucks2 && _watcherData != null)
            {
                try
                {
                    int pid = _watcherData.ProcessId;
                    App.Logger.WriteLine("Watcher", $"EuroTrucks2 rename mode active for PID {pid}");
                }
                catch { }
            }
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired || _watcherData is null)
                return;

            ActivityWatcher?.Start();

            while (true)
            {
            bool crashed = false;
            int tickCount = 0;

                while (true)
                {
                    Process? process = null;

                    try
                    {
                        process = Process.GetProcessById(_watcherData.ProcessId);
                    }
                    catch
                    {
                        break;
                    }

                    try
                    {
                        if (process.HasExited)
                        {
                            try { crashed = process.ExitCode != 0; }
                            catch (InvalidOperationException) { crashed = false; }
                            break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }

                    if (App.Settings.Prop.ShowResourceMonitor)
                    {
                        try
                        {
                            string mem = (process.WorkingSet64 / (1024.0 * 1024.0)).ToString("F1");
                            string tooltip = $"Roblox • {mem} MB";

                            if (!App.Settings.Prop.StreamingMode && ActivityWatcher?.InGame == true)
                            {
                                string sessionTime = $"{(DateTime.Now - (ActivityWatcher.Data.TimeJoined == default ? DateTime.Now : ActivityWatcher.Data.TimeJoined)):mm\\:ss}";
                                string currentTime = DateTime.Now.ToString("HH:mm");
                                tooltip = $"Roblox • {mem} MB • {sessionTime} • {currentTime}";
                            }

                            _notifyIcon?.UpdateTooltip(tooltip);
                        }
                        catch
                        {
                            // process might have exited during check
                        }
                    }

                    if (App.Settings.Prop.AntiAFK && (DateTime.Now - _lastAfkInput).TotalSeconds >= 30)
                    {
                        try
                        {
                            keybd_event(VK_F15, 0, 0, UIntPtr.Zero);
                            keybd_event(VK_F15, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                            _lastAfkInput = DateTime.Now;
                        }
                        catch
                        {
                            // ignore AFK input errors
                        }
                    }

                    // Periodic memory cleanup (every ~30 seconds)
                    tickCount++;
                    if (tickCount >= 30 && App.Settings.Prop.MemoryCleaner && ActivityWatcher?.InGame == true)
                    {
                        tickCount = 0;
                        try
                        {
                            using var proc = Process.GetProcessById(_watcherData!.ProcessId);
                            if (!proc.HasExited)
                            {
                                RobloxMemoryCleaner.OptimizeProcessMemory(_watcherData.ProcessId);
                            }
                        }
                        catch
                        {
                            // process might have exited
                        }
                    }

                    await Task.Delay(1000);
                }

                if (crashed)
                {
                    _consecutiveCrashes++;
                    App.Logger.WriteLine("Watcher::Run", $"Roblox crashed (crash #{_consecutiveCrashes})");
                }

                if (crashed && App.Settings.Prop.AutoRelaunchOnCrash && !string.IsNullOrEmpty(_watcherData.LaunchArgs))
                {
                    App.Logger.WriteLine("Watcher::Run", "Roblox crashed, relaunching...");
                    await Task.Delay(2000);

                    try
                    {
                        using var newProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = new RobloxPlayerData().ExecutablePath,
                            Arguments = _watcherData.LaunchArgs,
                            UseShellExecute = false
                        });

                        if (newProcess is not null)
                        {
                            _watcherData.ProcessId = newProcess.Id;
                            App.Logger.WriteLine("Watcher::Run", $"Relaunched Roblox (new PID {newProcess.Id}), continuing monitoring");

                            if (ActivityWatcher is not null)
                            {
                                ActivityWatcher.Dispose();

                                string logDirectory = Path.Combine(Paths.LocalAppData, "Roblox\\logs");
                                if (Directory.Exists(logDirectory))
                                {
                                    var logFile = new DirectoryInfo(logDirectory)
                                        .GetFiles()
                                        .Where(x => x.Name.Contains("Player", StringComparison.OrdinalIgnoreCase))
                                        .OrderByDescending(x => x.CreationTime)
                                        .FirstOrDefault();

                                    if (logFile is not null)
                                    {
                                        _watcherData.LogFile = logFile.FullName;
                                        App.Logger.WriteLine("Watcher::Run", $"New log file: {_watcherData.LogFile}");
                                    }
                                }
                            }

                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException("Watcher::Run::Relaunch", ex);
                    }
                }

                if (crashed && App.Settings.Prop.AutoRepairOnCrashLoop && _consecutiveCrashes >= 3)
                {
                    App.Logger.WriteLine("Watcher::Run", $"Auto-repair triggered after {_consecutiveCrashes} consecutive crashes");

                    App.State.Prop.ForceReinstall = true;
                    App.State.Save();

                    Process.Start(Paths.Process, App.LaunchSettings.ForceFlag.Active ? "-force" : "");
                }

                break;
            }

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            GoodbyeDPIManager.StopAll();

            if (App.Settings.Prop.AutoCleanRobloxCache)
                CleanHttpCache();

            if (App.LaunchSettings.TestModeFlag.Active)
                Process.Start(Paths.Process, "-settings -testmode");
        }

        private void OnServerBlocked(object? sender, string countryCode)
        {
            const string LOG_IDENT = "Watcher::OnServerBlocked";

            App.Logger.WriteLine(LOG_IDENT, $"Server blocked: {countryCode}");

            if (_notifyIcon is not null)
            {
                _notifyIcon.ShowAlert(
                    "Server Blocked",
                    $"Connected to a server in a blocked region ({countryCode}). Closing Roblox...",
                    5,
                    null
                );
            }

            KillRobloxProcess();
        }

        private async Task AutoRejoin(string deeplink)
        {
            const string LOG_IDENT = "Watcher::AutoRejoin";

            if ((DateTime.Now - _lastDisconnectTime).TotalSeconds < 10)
            {
                App.Logger.WriteLine(LOG_IDENT, "Rejoin cooldown active, skipping");
                return;
            }

            _lastDisconnectTime = DateTime.Now;
            App.Logger.WriteLine(LOG_IDENT, $"Auto-rejoining server: {deeplink}");

            await Task.Delay(3000);

            try
            {
                string playerPath = new RobloxPlayerData().ExecutablePath;
                if (File.Exists(playerPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = playerPath,
                        Arguments = deeplink,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void CleanHttpCache()
        {
            const string LOG_IDENT = "Watcher::CleanHttpCache";
            string cacheDir = Paths.RobloxHttpCache;

            if (!Directory.Exists(cacheDir))
                return;

            try
            {
                App.Logger.WriteLine(LOG_IDENT, $"Cleaning HTTP cache at {cacheDir}");

                foreach (string file in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
                {
                    try { File.Delete(file); }
                    catch { App.Logger.WriteLine(LOG_IDENT, $"Could not delete {file} (in use?)"); }
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

                App.Logger.WriteLine(LOG_IDENT, "HTTP cache cleaned successfully");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private void SaveServerHistory()
        {
            if (ActivityWatcher is null)
                return;

            try
            {
                string historyPath = Path.Combine(Paths.Base, "ServerHistory.json");
                var json = JsonSerializer.Serialize(ActivityWatcher.History, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(historyPath, json);
                App.Logger.WriteLine("Watcher::SaveServerHistory", $"Saved {ActivityWatcher.History.Count} history entries");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("Watcher::SaveServerHistory", "Failed to save server history");
                App.Logger.WriteException("Watcher::SaveServerHistory", ex);
            }
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            // save history on exit
            SaveServerHistory();

            _notifyIcon?.Dispose();
            RichPresence?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
