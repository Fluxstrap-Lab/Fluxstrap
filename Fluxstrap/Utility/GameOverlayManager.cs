using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Windows.Media.Control;
using Fluxstrap.UI.Elements.Overlay;

namespace Fluxstrap.Utility
{
    public static class GameOverlayManager
    {
        private const string LOG_IDENT = "GameOverlayManager";

        private static GameOverlayWindow? _overlayWindow;
        private static Integrations.ActivityWatcher? _activityWatcher;
        private static int _robloxPid;
        private static Timer? _positionTimer;
        private static Timer? _dataTimer;
        private static Timer? _topmostTimer;
        private static bool _isVisible;
        private static nint _robloxHwnd;
        private static int _currentFps;
        private static int _currentPing;
        private static long _currentMemoryMb;
        private static int _currentCpuPercent = -1;
        private static int _currentGpuPercent = -1;
        private static int _currentVolume = 100;
        private static string _serverLocation = "";
        private static List<string> _friendList = new();
        private static Process? _robloxProcess;
        private static DateTime _lastCpuTime = DateTime.MinValue;
        private static TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private static readonly object _lock = new();
        private static bool _threadRunning;
        private static bool _recording;

        // Win32 constants
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int HOTKEY_TOGGLE_OVERLAY = 1;
        private const int WM_HOTKEY = 0x0312;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const int SW_SHOWNA = 8;
        private const int SW_HIDE = 0;
        private static readonly nint HWND_TOPMOST = (nint)(-1);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(nint hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_SHIFT = 0x10;
        private const int VK_TAB = 0x09;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left; public int top; public int right; public int bottom; }

        public static bool IsVisible
        {
            get { lock (_lock) { return _isVisible; } }
            private set { lock (_lock) { _isVisible = value; } }
        }
        public static int CurrentFps => _currentFps;
        public static int CurrentPing => _currentPing;
        public static long CurrentMemoryMb => _currentMemoryMb;
        public static int CurrentCpuPercent => _currentCpuPercent;
        public static int CurrentGpuPercent => _currentGpuPercent;
        public static int CurrentVolume => _currentVolume;
        public static string ServerLocation => _serverLocation;
        public static List<string> FriendList => _friendList;

        public static string GameName => _activityWatcher?.Data?.UniverseDetails?.Data?.Name ?? "Roblox";
        public static string SessionDuration 
        {
            get 
            {
                if (_activityWatcher != null && _activityWatcher.Data != null && _activityWatcher.Data.TimeJoined != default)
                {
                    var span = DateTime.Now - _activityWatcher.Data.TimeJoined;
                    if (span.TotalHours >= 1) return $"{(int)span.TotalHours}h {span.Minutes}m";
                    return $"{span.Minutes} mins";
                }
                return "0 mins";
            }
        }

        public static string ContextPlaceId => _activityWatcher?.Data?.PlaceId.ToString() ?? "N/A";
        public static string ContextJobId => _activityWatcher?.Data?.JobId ?? "N/A";
        public static string ContextImageUrl => _activityWatcher?.Data?.UniverseDetails?.Thumbnail?.ImageUrl ?? "";
        
        public static string MediaTitle { get; private set; } = "No Media";
        public static string MediaArtist { get; private set; } = "";
        public static string MediaThumbnailPath { get; private set; } = "";

        public static event Action? OverlayDataChanged;

        public static void Start(Integrations.ActivityWatcher? activityWatcher = null)
        {
            _activityWatcher = activityWatcher;
            lock (_lock)
            {
                if (_threadRunning) return;
                _threadRunning = true;
            }

            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Starting overlay manager...");

                // Find Roblox process
                var proc = FindRobloxProcess();
                _robloxProcess = proc;
                _robloxPid = proc?.Id ?? 0;

                if (_robloxPid != 0)
                    GetRobloxWindowHandle();

                // Start the overlay thread (creates window + registers hotkey)
                StartOverlayThread();

                // If Roblox not found, watch for it
                if (_robloxPid == 0)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Roblox process not found, watching...");
                    WatchForRobloxProcess();
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static void Stop()
        {
            try
            {
                _watchCts?.Cancel();
                _watchCts?.Dispose();
                _watchCts = null;

                _positionTimer?.Dispose();
                _positionTimer = null;
                _dataTimer?.Dispose();
                _dataTimer = null;
                _topmostTimer?.Dispose();
                _topmostTimer = null;

                CloseOverlayWindow();
                lock (_lock)
                {
                    _robloxPid = 0;
                    _robloxHwnd = IntPtr.Zero;
                    _isVisible = false;
                    _threadRunning = false;
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static void ToggleVisibility()
        {
            try
            {
                UpdateRobloxHwnd();
                nint fg = GetForegroundWindow();
                bool robloxActive = (fg == _robloxHwnd && _robloxHwnd != IntPtr.Zero);

                if (_overlayWindow == null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Toggle: overlay window not ready yet");
                    return;
                }

                // Hide if Roblox not focused but overlay is showing
                if (!robloxActive && IsVisible)
                {
                    HideOverlayInternal();
                    return;
                }

                // Only toggle when Roblox is foreground
                if (!robloxActive) return;

                if (IsVisible)
                    HideOverlayInternal();
                else
                    ShowOverlayInternal();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static void ShowOverlayInternal()
        {
            if (_overlayWindow == null) return;
            try
            {
                _overlayWindow.Dispatcher.Invoke(() =>
                {
                    SyncWindowPosition();
                    _overlayWindow.Visibility = Visibility.Visible;
                    _overlayWindow.AnimateShow();
                    nint hwnd = new WindowInteropHelper(_overlayWindow).Handle;
                    SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOMOVE);
                    IsVisible = true;
                    App.Logger.WriteLine(LOG_IDENT, "Overlay shown");
                });
            }
            catch (Exception ex) { App.Logger.WriteException(LOG_IDENT, ex); }
        }

        private static void HideOverlayInternal()
        {
            if (_overlayWindow == null) return;
            try
            {
                _overlayWindow.Dispatcher.Invoke(() =>
                {
                    _overlayWindow.AnimateHide(() => 
                    {
                        _overlayWindow.Dispatcher.Invoke(() => {
                            _overlayWindow.Visibility = Visibility.Hidden;
                        });
                    });
                    IsVisible = false;
                    App.Logger.WriteLine(LOG_IDENT, "Overlay hidden");
                });
                // Refocus Roblox window after hiding
                RefocusRoblox();
            }
            catch { }
        }

        public static void RefocusRoblox()
        {
            try
            {
                UpdateRobloxHwnd();
                if (_robloxHwnd != IntPtr.Zero)
                {
                    SetForegroundWindow(_robloxHwnd);
                    App.Logger.WriteLine(LOG_IDENT, "Refocused Roblox window");
                }
            }
            catch { }
        }

        private static volatile bool _hotkeyPressed;
        private static Timer? _hotkeyTimer;

        private static void StartOverlayThread()
        {
            var readyEvent = new ManualResetEventSlim(false);

            var thread = new Thread(() =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Starting overlay STA thread...");

                // Ensure the overlay thread uses the same culture as Fluxstrap settings
                try
                {
                    if (App.Settings.Prop.Locale != "nil")
                    {
                        var culture = new System.Globalization.CultureInfo(App.Settings.Prop.Locale);
                        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    }
                }
                catch { }

                try
                {
                    // Create overlay window early on the STA thread so it owns the Dispatcher
                    _overlayWindow = new GameOverlayWindow();
                    _overlayWindow.Visibility = Visibility.Hidden; // hidden until user presses hotkey
                    nint overlayHwnd = new WindowInteropHelper(_overlayWindow).EnsureHandle();

                    int exStyle = GetWindowLong(overlayHwnd, GWL_EXSTYLE);
                    exStyle |= WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                    SetWindowLong(overlayHwnd, GWL_EXSTYLE, exStyle);

                    App.Logger.WriteLine(LOG_IDENT, "Overlay window created on STA thread");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                    return;
                }
                finally
                {
                    readyEvent.Set();
                }

                // Start data and position timers (they dispatch back via Dispatcher)
                StartDataRefresh();
                StartPositionTracking();
                StartTopmostEnforcement();

                // Start hotkey polling timer — runs on ThreadPool, dispatches to STA
                _hotkeyPressed = false;
                _hotkeyTimer?.Dispose();
                _hotkeyTimer = new Timer(_ =>
                {
                    try
                    {
                        bool isShiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                        bool isTabPressed = (GetAsyncKeyState(VK_TAB) & 0x8000) != 0;

                        if (isShiftPressed && isTabPressed)
                        {
                            if (!_hotkeyPressed)
                            {
                                _hotkeyPressed = true;
                                // Dispatch to the STA thread where the window lives
                                _overlayWindow?.Dispatcher.Invoke(() => HandleHotkeyToggle());
                            }
                        }
                        else
                        {
                            _hotkeyPressed = false;
                        }
                    }
                    catch { }
                }, null, 0, 50);

                // Run WPF message pump — blocks until window is closed
                System.Windows.Threading.Dispatcher.Run();

                // Cleanup after Dispatcher exits
                _hotkeyTimer?.Dispose();
                _hotkeyTimer = null;
                _overlayWindow = null;
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            // Wait for window to be created before returning
            if (!readyEvent.Wait(5000))
                App.Logger.WriteLine(LOG_IDENT, "Timed out waiting for overlay STA thread init");
        }

        private static void HandleHotkeyToggle()
        {
            // Lazy load the WPF window if it doesn't exist yet
            if (_overlayWindow == null)
            {
                App.Logger.WriteLine(LOG_IDENT, "Lazy-loading overlay window for first use");
                try
                {
                    _overlayWindow = new GameOverlayWindow();
                    nint overlayHwnd = new WindowInteropHelper(_overlayWindow).EnsureHandle();
                    
                    int exStyle = GetWindowLong(overlayHwnd, GWL_EXSTYLE);
                    exStyle |= WS_EX_LAYERED;
                    exStyle |= WS_EX_NOACTIVATE;
                    exStyle |= WS_EX_TOOLWINDOW;
                    SetWindowLong(overlayHwnd, GWL_EXSTYLE, exStyle);
                    
                    App.Logger.WriteLine(LOG_IDENT, "Lazy-load complete");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT + "::LazyInit", ex);
                    return;
                }
            }

            if (IsVisible)
            {
                App.Logger.WriteLine(LOG_IDENT, "Hiding overlay");
                _overlayWindow.AnimateHide(() =>
                {
                    _overlayWindow?.Dispatcher.Invoke(() =>
                    {
                        if (_overlayWindow != null)
                            _overlayWindow.Visibility = Visibility.Hidden;
                    });
                });
                IsVisible = false;
                RefocusRoblox();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Showing overlay");
                SyncWindowPosition();
                _overlayWindow.Visibility = Visibility.Visible;
                _overlayWindow.AnimateShow();
                
                nint wpfHwnd = new WindowInteropHelper(_overlayWindow).Handle;
                SetWindowPos(wpfHwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOMOVE);
                IsVisible = true;
            }
        }

        private static void SyncWindowPosition()
        {
            UpdateRobloxHwnd();
            if (_robloxHwnd == IntPtr.Zero || _overlayWindow == null) return;
            try
            {
                if (!GetWindowRect(_robloxHwnd, out var rect)) return;
                int rw = rect.right - rect.left;
                int rh = rect.bottom - rect.top;
                if (rw <= 0 || rh <= 0) return;

                // Use BeginInvoke to prevent deadlocks from timer callbacks
                _overlayWindow.Dispatcher.BeginInvoke(() =>
                {
                    if (_overlayWindow == null) return;
                    _overlayWindow.Left = rect.left;
                    _overlayWindow.Top = rect.top;
                    _overlayWindow.Width = rw;
                    _overlayWindow.Height = rh;
                });
            }
            catch { }
        }

        private static void StartPositionTracking()
        {
            _positionTimer?.Dispose();
            _positionTimer = new Timer(_ =>
            {
                try
                {
                    if (_robloxPid == 0) return;
                    
                    bool hasExited = true;
                    try
                    {
                        using var p = Process.GetProcessById(_robloxPid);
                        hasExited = p.HasExited;
                    }
                    catch { }

                    if (hasExited)
                    {
                        Stop();
                        return;
                    }
                    
                    if (IsVisible)
                        SyncWindowPosition();
                }
                catch { }
            }, null, 1000, 200);
        }

        /// <summary>
        /// Periodically re-enforces HWND_TOPMOST on the overlay window.
        /// Games (including Roblox) can reclaim Z-order, pushing our window behind.
        /// This timer ensures the overlay stays on top when visible.
        /// </summary>
        private static void StartTopmostEnforcement()
        {
            _topmostTimer?.Dispose();
            _topmostTimer = new Timer(_ =>
            {
                try
                {
                    if (!IsVisible || _overlayWindow == null) return;

                    _overlayWindow.Dispatcher.BeginInvoke(() =>
                    {
                        if (_overlayWindow == null) return;
                        try
                        {
                            nint hwnd = new WindowInteropHelper(_overlayWindow).Handle;
                            if (hwnd != IntPtr.Zero)
                            {
                                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                                    SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
                            }
                        }
                        catch { }
                    });
                }
                catch { }
            }, null, 2000, 500);
        }

        private static void StartDataRefresh()
        {
            _dataTimer?.Dispose();
            _dataTimer = new Timer(_ =>
            {
                try { RefreshData(); }
                catch { }
            }, null, 2000, 1000);
        }

        private static async void RefreshData()
        {
            if (_robloxPid == 0) return;
            
            try 
            {
                using var p = Process.GetProcessById(_robloxPid);
                if (p.HasExited) return;
                p.Refresh();
                _currentMemoryMb = p.WorkingSet64 / 1024 / 1024;

                _currentCpuPercent = CalculateCpuUsage(p);
            }
            catch { return; }
            _currentFps = ReadFpsFromLog();
            _currentPing = ReadPingFromLog();

            if (_activityWatcher != null && _activityWatcher.Data != null)
            {
                try 
                {
                    string? loc = await _activityWatcher.Data.QueryServerLocation();
                    _serverLocation = string.IsNullOrEmpty(loc) ? "Unknown" : loc;
                }
                catch { }
            }

            try { SmtcHelper.UpdateMediaProperties(); } catch { }
            OverlayDataChanged?.Invoke();
        }

        private static class SmtcHelper
        {
            public static async void UpdateMediaProperties()
            {
                try
                {
                    var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    var session = sessionManager.GetCurrentSession();
                    if (session == null) {
                        GameOverlayManager.UpdateMediaData("No Media", "", "");
                        return;
                    }

                    var mediaProperties = await session.TryGetMediaPropertiesAsync();
                    string title = mediaProperties?.Title ?? "No Media";
                    string artist = mediaProperties?.Artist ?? "";
                    string tempPath = GameOverlayManager.MediaThumbnailPath;

                    // CRITICAL FIX: Only spin up new WPF Image streams if the song actually changed!
                    // Generating new GUID files every second causes chaotic wpfgfx_cor3.dll crashes.
                    if (title == GameOverlayManager.MediaTitle && artist == GameOverlayManager.MediaArtist)
                        return;

                    if (mediaProperties != null && mediaProperties.Thumbnail != null)
                    {
                        using var stream = await mediaProperties.Thumbnail.OpenReadAsync();
                        using var dataReader = new Windows.Storage.Streams.DataReader(stream);
                        await dataReader.LoadAsync((uint)stream.Size);
                        if (stream.Size > 0)
                        {
                            byte[] buffer = new byte[(int)stream.Size];
                            dataReader.ReadBytes(buffer);
                            string tempFile = Path.Combine(Path.GetTempPath(), $"fluxstrap_smtc_thumb_{Guid.NewGuid():N}.jpg");
                            File.WriteAllBytes(tempFile, buffer);
                            tempPath = tempFile;
                        }
                    }
                    GameOverlayManager.UpdateMediaData(title, artist, tempPath);
                }
                catch { }
            }
        }

        public static void UpdateMediaData(string title, string artist, string thumbPath)
        {
            MediaTitle = title;
            MediaArtist = artist;
            MediaThumbnailPath = thumbPath;
        }

        private static int CalculateCpuUsage(Process process)
        {
            try
            {
                var now = DateTime.UtcNow;
                var totalTime = process.TotalProcessorTime;

                if (_lastCpuTime != DateTime.MinValue)
                {
                    double elapsedMs = (now - _lastCpuTime).TotalMilliseconds;
                    double cpuMs = (totalTime - _lastTotalProcessorTime).TotalMilliseconds;

                    if (elapsedMs > 0)
                    {
                        int pct = (int)((cpuMs / (elapsedMs * Environment.ProcessorCount)) * 100);
                        _lastCpuTime = now;
                        _lastTotalProcessorTime = totalTime;
                        return Math.Clamp(pct, 0, 100);
                    }
                }

                _lastCpuTime = now;
                _lastTotalProcessorTime = totalTime;
                return _currentCpuPercent;
            }
            catch
            {
                return _currentCpuPercent;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(nint hWnd, int Msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int WM_APPCOMMAND = 0x0319;
        private const int APPCOMMAND_VOLUME_UP = 0x0a;
        private const int APPCOMMAND_VOLUME_DOWN = 0x09;
        private const int APPCOMMAND_VOLUME_MUTE = 0x08;

        private const byte VK_VOLUME_MUTE = 0xAD;
        private const byte VK_VOLUME_DOWN = 0xAE;
        private const byte VK_VOLUME_UP = 0xAF;

        public static void SetSystemVolume(int level)
        {
            try
            {
                _currentVolume = Math.Clamp(level, 0, 100);

                // Reset to 0 first (mute by holding down for a while)
                for (int i = 0; i < 50; i++)
                {
                    keybd_event(VK_VOLUME_DOWN, 0, 0, 0);
                }
                Thread.Sleep(50);
                // Set to desired level (each key press ~2% volume)
                for (int i = 0; i < _currentVolume / 2; i++)
                {
                    keybd_event(VK_VOLUME_UP, 0, 0, 0);
                }
            }
            catch { }
        }

        public static void StartRecording()
        {
            _recording = true;
            App.Logger.WriteLine(LOG_IDENT, "Screen recording started (placeholder)");
        }

        public static string StopRecording()
        {
            _recording = false;
            string path = Path.Combine(Paths.Base, "Recordings");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            App.Logger.WriteLine(LOG_IDENT, "Screen recording stopped (placeholder)");
            return path;
        }

        public static void KillRoblox()
        {
            try
            {
                if (_robloxPid != 0)
                {
                    using var p = Process.GetProcessById(_robloxPid);
                    p.Kill();
                }
                HideOverlayInternal();
            }
            catch { }
        }

        private static int ReadFpsFromLog()
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs");
                if (!Directory.Exists(logDir)) return _currentFps;
                var logFile = Directory.GetFiles(logDir, "*.log").OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
                if (logFile == null) return _currentFps;

                using var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long pos = Math.Max(0, stream.Length - 50000);
                stream.Position = pos;
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();

                var m = Regex.Match(content, @"FPS\s*[:]\s*(\d+)", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int v)) return v;
                return _currentFps;
            }
            catch { return _currentFps; }
        }

        private static int ReadPingFromLog()
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs");
                if (!Directory.Exists(logDir)) return _currentPing;
                var logFile = Directory.GetFiles(logDir, "*.log").OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
                if (logFile == null) return _currentPing;

                using var stream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long pos = Math.Max(0, stream.Length - 50000);
                stream.Position = pos;
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();

                var m = Regex.Match(content, @"Ping\s*[:]\s*(\d+)", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int v)) return v;
                return _currentPing;
            }
            catch { return _currentPing; }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

        public static void MediaPrev() => keybd_event(0xB1, 0, 0, 0);
        public static void MediaPlayPause() => keybd_event(0xB3, 0, 0, 0);
        public static void MediaNext() => keybd_event(0xB0, 0, 0, 0);

        private static void CloseOverlayWindow()
        {
            if (_overlayWindow == null) return;
            try
            {
                _overlayWindow.Dispatcher.Invoke(() =>
                {
                    _overlayWindow.Close();
                    _overlayWindow = null;
                });
            }
            catch { }
        }

        private static void UpdateRobloxHwnd()
        {
            if (_robloxProcess == null) return;
            try
            {
                _robloxProcess.Refresh();
                if (_robloxProcess.MainWindowHandle != IntPtr.Zero)
                {
                    _robloxHwnd = _robloxProcess.MainWindowHandle;
                }
            }
            catch { }
        }

        private static bool GetRobloxWindowHandle()
        {
            if (_robloxProcess == null) return false;
            try
            {
                _robloxProcess.Refresh();
                if (_robloxProcess.MainWindowHandle != IntPtr.Zero)
                {
                    _robloxHwnd = _robloxProcess.MainWindowHandle;
                    return true;
                }
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    _robloxProcess.Refresh();
                    if (_robloxProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        _robloxHwnd = _robloxProcess.MainWindowHandle;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static Process? FindRobloxProcess()
        {
            foreach (var name in new[] { "RobloxPlayerBeta", "RobloxStudioBeta", "Roblox" })
            {
                try
                {
                    var processes = Process.GetProcessesByName(name);
                    if (processes.Length > 0)
                    {
                        foreach (var p in processes)
                        {
                            try { if (!p.HasExited) return p; }
                            catch { }
                        }
                        return processes[0];
                    }
                }
                catch { }
            }
            return null;
        }

        private static CancellationTokenSource? _watchCts;

        private static void WatchForRobloxProcess()
        {
            _watchCts?.Cancel();
            _watchCts = new CancellationTokenSource();
            var token = _watchCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var proc = FindRobloxProcess();
                    if (proc != null)
                    {
                        _robloxProcess = proc;
                        _robloxPid = proc.Id;
                        if (GetRobloxWindowHandle())
                            return;
                    }
                    try { await Task.Delay(2000, token); }
                    catch (TaskCanceledException) { return; }
                }
            });
        }
    }
}
