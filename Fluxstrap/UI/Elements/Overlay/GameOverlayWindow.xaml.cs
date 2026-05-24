using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Fluxstrap.Resources;
using Fluxstrap.Utility;

namespace Fluxstrap.UI.Elements.Overlay
{
    public partial class GameOverlayWindow : Window
    {
        private DispatcherTimer? _dataTimer;
        private DispatcherTimer? _toastTimer;

        private static readonly SolidColorBrush BrushGreen = new(Color.FromRgb(0x00, 0xE6, 0x76));
        private static readonly SolidColorBrush BrushYellow = new(Color.FromRgb(0xFF, 0xD6, 0x00));
        private static readonly SolidColorBrush BrushRed = new(Color.FromRgb(0xFF, 0x52, 0x52));
        private static readonly SolidColorBrush BrushAccent = new(Color.FromRgb(0x00, 0xA2, 0xFF));
        private static readonly SolidColorBrush BrushDim = new(Color.FromArgb(0x50, 0x80, 0x80, 0x90));
        private static readonly SolidColorBrush BrushActiveText = new(Color.FromArgb(0xA0, 0xA0, 0xA0, 0xB0));
        private static readonly SolidColorBrush BrushDimText = new(Color.FromArgb(0x50, 0x80, 0x80, 0x90));

        static GameOverlayWindow()
        {
            BrushGreen.Freeze();
            BrushYellow.Freeze();
            BrushRed.Freeze();
            BrushAccent.Freeze();
            BrushDim.Freeze();
            BrushActiveText.Freeze();
            BrushDimText.Freeze();
        }

        public GameOverlayWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::Constructor", ex);
                return;
            }

            try
            {
                GameOverlayManager.OverlayDataChanged += OnOverlayDataChanged;
                _dataTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                _dataTimer.Tick += UpdateOverlayData;
                _dataTimer.Start();
                UpdateStatusIndicators();
                UpdateOverlayData(null, EventArgs.Empty);
                UpdateAccentColor();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::Constructor", ex);
            }
        }

        private void OnOverlayDataChanged()
        {
            try { Dispatcher.BeginInvoke(() => UpdateOverlayData(null, EventArgs.Empty)); }
            catch { }
        }

        private void UpdateAccentColor()
        {
            try
            {
                string accent = App.Settings.Prop.OverlayAccentColor;
                if (string.IsNullOrEmpty(accent)) return;

                Color color = (Color)ColorConverter.ConvertFromString(accent);
                Resources["AccentColor"] = color;
                Resources["AccentBrush"] = new SolidColorBrush(color);

                if (LogoBorder != null)
                {
                    LogoBorder.Background = new LinearGradientBrush(color, Color.FromRgb(0x7A, 0x39, 0xFB), 0);
                }
            }
            catch { }
        }

        private void ForceQuit_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { GameOverlayManager.KillRoblox(); } catch { }
        }

        private void ReturnToGame_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { GameOverlayManager.ToggleVisibility(); } catch { }
        }

        private void MediaPrev_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { GameOverlayManager.MediaPrev(); } catch { }
        }

        private void MediaPlayPause_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { GameOverlayManager.MediaPlayPause(); } catch { }
        }

        private void MediaNext_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { GameOverlayManager.MediaNext(); } catch { }
        }

        private void Screenshot_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string screenshotDir = Path.Combine(Paths.Base, "Screenshots");
                if (!Directory.Exists(screenshotDir))
                    Directory.CreateDirectory(screenshotDir);

                string fileName = $"Fluxstrap_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
                string filePath = Path.Combine(screenshotDir, fileName);

                var screenBounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds
                    ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);

                using (var bitmap = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height))
                {
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(screenBounds.Left, screenBounds.Top, 0, 0, screenBounds.Size);
                    }
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }

                App.Logger.WriteLine("GameOverlayWindow", $"Screenshot saved to {filePath}");
                ShowToast(string.Format(Strings.Overlay_ScreenshotSaved, fileName));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::Screenshot_Click", ex);
                ShowToast(Strings.Overlay_ScreenshotFailed);
            }
        }

        private bool _isRecording = false;

        private void Recording_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!_isRecording)
                {
                    GameOverlayManager.StartRecording();
                    _isRecording = true;
                    RecordingIcon.Text = "⏹";
                    UI_RecordingLabel.Text = Strings.Overlay_StopRecording;
                    ShowToast(Strings.Overlay_RecordingStarted);
                }
                else
                {
                    string path = GameOverlayManager.StopRecording();
                    _isRecording = false;
                    RecordingIcon.Text = "⏺";
                    UI_RecordingLabel.Text = Strings.Overlay_StartRecording;
                    ShowToast(Strings.Overlay_RecordingStopped);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::Recording_Click", ex);
            }
        }

        private void OpenModDir_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string modsPath = Paths.Modifications;
                if (!Directory.Exists(modsPath))
                    Directory.CreateDirectory(modsPath);
                Process.Start(new ProcessStartInfo { FileName = modsPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::OpenModDir_Click", ex);
            }
        }

        private void OpenLogDir_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs");
                if (Directory.Exists(logDir))
                    Process.Start(new ProcessStartInfo { FileName = logDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::OpenLogDir_Click", ex);
            }
        }

        private void OpenScreenshotsDir_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string screenshotDir = Path.Combine(Paths.Base, "Screenshots");
                if (!Directory.Exists(screenshotDir))
                    Directory.CreateDirectory(screenshotDir);
                Process.Start(new ProcessStartInfo { FileName = screenshotDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("GameOverlayWindow::OpenScreenshotsDir_Click", ex);
            }
        }

        private void CopyInvite_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string placeId = GameOverlayManager.ContextPlaceId;
                string jobId = GameOverlayManager.ContextJobId;
                string invite = $"roblox://experiences/start?placeId={placeId}&gameInstanceId={jobId}";
                System.Windows.Clipboard.SetText(invite);

                if (UI_CopyInviteLabel != null)
                {
                    UI_CopyInviteLabel.Text = Strings.Overlay_Copied;
                    var resetTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    resetTimer.Tick += (s, ev) =>
                    {
                        UI_CopyInviteLabel.Text = Strings.Overlay_CopyInvite;
                        resetTimer.Stop();
                    };
                    resetTimer.Start();
                }
                ShowToast(Strings.Overlay_CopiedInviteLink);
            }
            catch { }
        }

        private void CopyPlaceId_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Clipboard.SetText(GameOverlayManager.ContextPlaceId);
                ShowToast(Strings.Overlay_CopiedPlaceId);
            }
            catch { }
        }

        private void CopyJobId_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Clipboard.SetText(GameOverlayManager.ContextJobId);
                ShowToast(Strings.Overlay_CopiedJobId);
            }
            catch { }
        }

        private void ToggleFpsUnlock_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                bool current = App.Settings.Prop.UnlockFPS;
                App.Settings.Prop.UnlockFPS = !current;
                App.Settings.Save();
                UpdateStatusIndicators();
                ShowToast(!current ? "FPS Unlock: ON" : "FPS Unlock: OFF");
            }
            catch { }
        }

        private void ToggleAntiAfk_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                bool current = App.Settings.Prop.AntiAFK;
                App.Settings.Prop.AntiAFK = !current;
                App.Settings.Save();
                UpdateStatusIndicators();
                ShowToast(!current ? "Anti-AFK: ON" : "Anti-AFK: OFF");
            }
            catch { }
        }

        private void ToggleRpc_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                bool current = App.Settings.Prop.UseDiscordRichPresence;
                App.Settings.Prop.UseDiscordRichPresence = !current;
                App.Settings.Save();
                UpdateStatusIndicators();
                ShowToast(!current ? "Rich Presence: ON" : "Rich Presence: OFF");
            }
            catch { }
        }

        private int _volumeLevel = 100;

        private void VolumeSlider_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var pos = e.GetPosition(VolumeFill);
                double pct = Math.Clamp(pos.X / VolumeFill.ActualWidth, 0, 1);
                _volumeLevel = (int)(pct * 100);
                GameOverlayManager.SetSystemVolume(_volumeLevel);
                UpdateVolumeDisplay();
            }
            catch { }
        }

        private void UpdateVolumeDisplay()
        {
            if (VolumeFill == null || VolumeCard == null) return;
            VolumeFill.Width = VolumeFill.Parent is FrameworkElement parent
                ? parent.ActualWidth * (_volumeLevel / 100.0) : _volumeLevel;

            if (_volumeLevel <= 0) VolumeIconText.Text = "🔇";
            else if (_volumeLevel < 30) VolumeIconText.Text = "🔈";
            else if (_volumeLevel < 70) VolumeIconText.Text = "🔉";
            else VolumeIconText.Text = "🔊";
        }

        public void AnimateShow()
        {
            try
            {
                var fadeAnim = new DoubleAnimation
                {
                    From = 0.0,
                    To = App.Settings.Prop.OverlayOpacity,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                this.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
            }
            catch { }
        }

        public void AnimateHide(Action onCompleted)
        {
            try
            {
                var fadeAnim = new DoubleAnimation
                {
                    From = this.Opacity,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                fadeAnim.Completed += (s, e) => onCompleted?.Invoke();
                this.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
            }
            catch
            {
                onCompleted?.Invoke();
            }
        }

        private void ShowToast(string message)
        {
            try
            {
                if (ScreenshotToast == null || ToastText == null) return;

                _toastTimer?.Stop();
                ToastText.Text = message;
                ScreenshotToast.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                ScreenshotToast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _toastTimer.Tick += (s, e) =>
                {
                    _toastTimer.Stop();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                    fadeOut.Completed += (_, _) =>
                    {
                        try { ScreenshotToast.Visibility = Visibility.Collapsed; } catch { }
                    };
                    ScreenshotToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                };
                _toastTimer.Start();
            }
            catch { }
        }

        private void UpdateStatusIndicators()
        {
            try
            {
                bool fpsUnlocked = App.Settings.Prop.UnlockFPS;
                StatusDot_FPS.Fill = fpsUnlocked ? BrushGreen : BrushDim;
                StatusText_FPS.Text = fpsUnlocked
                    ? string.Format(Strings.Overlay_FpsUnlockOn, App.Settings.Prop.FPSCap)
                    : Strings.Overlay_FpsUnlockOff;
                StatusText_FPS.Foreground = fpsUnlocked ? BrushActiveText : BrushDimText;

                bool antiAfk = App.Settings.Prop.AntiAFK;
                StatusDot_AntiAFK.Fill = antiAfk ? BrushGreen : BrushDim;
                StatusText_AntiAFK.Text = antiAfk ? Strings.Overlay_AntiAfkActive : Strings.Overlay_AntiAfkOff;
                StatusText_AntiAFK.Foreground = antiAfk ? BrushActiveText : BrushDimText;

                bool rpc = App.Settings.Prop.UseDiscordRichPresence;
                StatusDot_RPC.Fill = rpc ? BrushAccent : BrushDim;
                StatusText_RPC.Text = rpc ? Strings.Overlay_RichPresenceActive : Strings.Overlay_RichPresenceOff;
                StatusText_RPC.Foreground = rpc ? BrushActiveText : BrushDimText;
            }
            catch { }
        }

        private string? _lastImageUrl = null;
        private string? _lastSmtcUrl = null;
        private readonly List<int> _fpsHistory = new(60);

        private void UpdateOverlayData(object? sender, EventArgs e)
        {
            try
            {
                ClockText.Text = DateTime.Now.ToString("HH:mm");

                int fps = GameOverlayManager.CurrentFps;
                FpsText.Text = fps.ToString();
                SolidColorBrush fpsBrush;
                if (fps >= 50)
                    fpsBrush = BrushGreen;
                else if (fps >= 30)
                    fpsBrush = BrushYellow;
                else
                    fpsBrush = BrushRed;
                FpsText.Foreground = fpsBrush;
                FpsUnitText.Foreground = fpsBrush;

                PingText.Text = string.Format(Strings.Overlay_Ping, GameOverlayManager.CurrentPing);
                MemoryText.Text = string.Format(Strings.Overlay_Memory, GameOverlayManager.CurrentMemoryMb);
                RamText.Text = $"{GameOverlayManager.CurrentMemoryMb} MB";

                int cpu = GameOverlayManager.CurrentCpuPercent;
                int gpu = GameOverlayManager.CurrentGpuPercent;
                CpuText.Text = cpu >= 0 ? string.Format(Strings.Overlay_Cpu, cpu) : "";
                GpuText.Text = gpu >= 0 ? string.Format(Strings.Overlay_Gpu, gpu) : "";

                string server = GameOverlayManager.ServerLocation;
                ServerText.Text = string.IsNullOrEmpty(server) ? Strings.Overlay_Unknown : server;

                GameNameText.Text = GameOverlayManager.GameName;
                ActivityGameName.Text = GameOverlayManager.GameName;
                SessionChipText.Text = GameOverlayManager.SessionDuration;

                PlaceIdText.Text = GameOverlayManager.ContextPlaceId;
                JobIdText.Text = GameOverlayManager.ContextJobId;

                MediaTitleText.Text = string.IsNullOrEmpty(GameOverlayManager.MediaTitle)
                    ? Strings.Overlay_NoMedia : GameOverlayManager.MediaTitle;
                MediaArtistText.Text = GameOverlayManager.MediaArtist;

                _volumeLevel = GameOverlayManager.CurrentVolume;
                UpdateVolumeDisplay();

                string imageUrl = GameOverlayManager.ContextImageUrl;
                if (!string.IsNullOrEmpty(imageUrl) && imageUrl != _lastImageUrl)
                {
                    ActivityThumbnail.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imageUrl));
                    _lastImageUrl = imageUrl;
                }

                string smtcUrl = GameOverlayManager.MediaThumbnailPath;
                if (!string.IsNullOrEmpty(smtcUrl) && smtcUrl != _lastSmtcUrl && File.Exists(smtcUrl))
                {
                    try { if (_lastSmtcUrl != null) File.Delete(_lastSmtcUrl); } catch { }

                    var bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(smtcUrl);
                    bmp.EndInit();
                    MediaThumbnail.Source = bmp;
                    _lastSmtcUrl = smtcUrl;
                }

                if (fps >= 50)
                {
                    UI_CurrentStatusLabel.Text = Strings.Overlay_RunningSmoothly;
                    StatusDot_Running.Fill = BrushGreen;
                }
                else if (fps >= 30)
                {
                    UI_CurrentStatusLabel.Text = Strings.Overlay_ModeratePerformance;
                    StatusDot_Running.Fill = BrushYellow;
                }
                else if (fps > 0)
                {
                    UI_CurrentStatusLabel.Text = Strings.Overlay_LowPerformance;
                    StatusDot_Running.Fill = BrushRed;
                }

                _fpsHistory.Add(fps);
                if (_fpsHistory.Count > 60)
                    _fpsHistory.RemoveAt(0);
                UpdateFpsGraph();
            }
            catch { }
        }

        private void UpdateFpsGraph()
        {
            try
            {
                if (FpsGraphLine == null || _fpsHistory.Count < 2) return;

                var points = new PointCollection();
                double w = FpsGraphLine.ActualWidth > 0 ? FpsGraphLine.ActualWidth : 200;
                double h = FpsGraphLine.ActualHeight > 0 ? FpsGraphLine.ActualHeight : 60;
                int count = _fpsHistory.Count;

                for (int i = 0; i < count; i++)
                {
                    double x = (double)i / (count - 1) * w;
                    double y = h - Math.Clamp(_fpsHistory[i] / 240.0, 0, 1) * h;
                    points.Add(new Point(x, y));
                }

                FpsGraphLine.Points = points;
            }
            catch { }
        }
    }
}
