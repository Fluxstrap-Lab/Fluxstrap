using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using Fluxstrap.Integrations;
using Fluxstrap.Utility;

namespace Fluxstrap.UI.Elements.ContextMenu
{
    /// <summary>
    /// Interaction logic for NotifyIconMenu.xaml
    /// </summary>
    public partial class MenuContainer
    {
        private readonly Watcher _watcher;

        private HotkeyManager? _hotkeyManager;

        private ActivityWatcher? _activityWatcher => _watcher.ActivityWatcher;

        private ServerInformation? _serverInformationWindow;

        private ServerHistory? _gameHistoryWindow;

        public MenuContainer(Watcher watcher)
        {
            InitializeComponent();

            _watcher = watcher;

            if (_activityWatcher is not null)
            {
                _activityWatcher.OnLogOpen += ActivityWatcher_OnLogOpen;
                _activityWatcher.OnGameJoin += ActivityWatcher_OnGameJoin;
                _activityWatcher.OnGameLeave += ActivityWatcher_OnGameLeave;

                if (!App.Settings.Prop.UseDisableAppPatch)
                    GameHistoryMenuItem.Visibility = Visibility.Visible;
            }

            if (_watcher.RichPresence is not null)
                RichPresenceMenuItem.Visibility = Visibility.Visible;

            VersionTextBlock.Text = $"{App.ProjectName} v{App.Version}";
        }

        public void ShowServerInformationWindow()
        {
            if (_serverInformationWindow is null)
            {
                _serverInformationWindow = new(_watcher);
                _serverInformationWindow.Closed += (_, _) => _serverInformationWindow = null;
            }

            if (!_serverInformationWindow.IsVisible)
                _serverInformationWindow.ShowDialog();
            else
                _serverInformationWindow.Activate();
        }

        public void ActivityWatcher_OnLogOpen(object? sender, EventArgs e) => 
            Dispatcher.Invoke(() => LogTracerMenuItem.Visibility = Visibility.Visible);

        public void ActivityWatcher_OnGameJoin(object? sender, EventArgs e)
        {
            if (_activityWatcher is null)
                return;

            Dispatcher.Invoke(() => {
                if (_activityWatcher.Data.ServerType == ServerType.Public)
                    InviteDeeplinkMenuItem.Visibility = Visibility.Visible;

                ServerDetailsMenuItem.Visibility = Visibility.Visible;
            });
        }

        public void ActivityWatcher_OnGameLeave(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                InviteDeeplinkMenuItem.Visibility = Visibility.Collapsed;
                ServerDetailsMenuItem.Visibility = Visibility.Collapsed;

                _serverInformationWindow?.Close();
            });
        }

        private void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            HWND hWnd = (HWND)new WindowInteropHelper(this).Handle;

            int exStyle = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            exStyle |= 0x00000080;
            PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);

            _hotkeyManager = new HotkeyManager((IntPtr)hWnd);

            if (App.Settings.Prop.EnableGlobalHotkeys)
            {
                _hotkeyManager.RejoinLastServerRequested += (_, _) =>
                {
                    if (_activityWatcher?.History.Count > 0)
                    {
                        var last = _activityWatcher.History[0];
                        string deeplink = $"roblox-player:1+launchmode:play+placeId:{last.PlaceId}";
                        Process.Start(new ProcessStartInfo { FileName = deeplink, UseShellExecute = true });
                    }
                };
                _hotkeyManager.KillRobloxRequested += (_, _) => _watcher.KillRobloxProcess();
                _hotkeyManager.ToggleFPSUnlockRequested += (_, _) =>
                {
                    App.Settings.Prop.UnlockFPS = !App.Settings.Prop.UnlockFPS;
                    App.Settings.Save();
                };
            }

            _hotkeyManager.Register();
        }

        private void Window_Closed(object sender, EventArgs e) => App.Logger.WriteLine("MenuContainer::Window_Closed", "Context menu container closed");

        private void RichPresenceMenuItem_Click(object sender, RoutedEventArgs e) => _watcher.RichPresence?.SetVisibility(((MenuItem)sender).IsChecked);

        private void InviteDeeplinkMenuItem_Click(object sender, RoutedEventArgs e) => Clipboard.SetDataObject(_activityWatcher?.Data.GetInviteDeeplink());

        private void ServerDetailsMenuItem_Click(object sender, RoutedEventArgs e) => ShowServerInformationWindow();

        private void LogTracerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string? location = _activityWatcher?.LogLocation;

            if (location is not null)
                Utilities.ShellExecute(location);
        }

        private void CloseRobloxMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = Frontend.ShowMessageBox(
                Strings.ContextMenu_CloseRobloxMessage,
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo
            );

            if (result != MessageBoxResult.Yes)
                return;

            _watcher.KillRobloxProcess();
        }

        private void QuickJoinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string? placeId = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter a Place ID to join:",
                "Quick Join",
                ""
            );

            if (string.IsNullOrWhiteSpace(placeId))
                return;

            if (!long.TryParse(placeId, out _))
            {
                Frontend.ShowMessageBox("Invalid Place ID. Please enter a numeric value.", MessageBoxImage.Error);
                return;
            }

            string deeplink = $"roblox-player:1+launchmode:play+placeId:{placeId}";
            Process.Start(new ProcessStartInfo { FileName = deeplink, UseShellExecute = true });
        }

        private void JoinLastServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_activityWatcher is null)
                throw new ArgumentNullException(nameof(_activityWatcher));

            if (_gameHistoryWindow is null)
            {
                _gameHistoryWindow = new(_activityWatcher);
                _gameHistoryWindow.Closed += (_, _) => _gameHistoryWindow = null;
            }

            if (!_gameHistoryWindow.IsVisible)
                _gameHistoryWindow.ShowDialog();
            else
                _gameHistoryWindow.Activate();
        }
    }
}
