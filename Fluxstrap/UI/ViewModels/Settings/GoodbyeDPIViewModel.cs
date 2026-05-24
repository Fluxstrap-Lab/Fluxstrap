using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class GoodbyeDPIViewModel : NotifyPropertyChangedViewModel, IDisposable
    {
        private GoodbyeDPIStatus _status = GoodbyeDPIStatus.Stopped;
        private string _log = "";
        private bool _autoStartWithRoblox;

        private bool _isDownloaded;
        private Timer? _statusTimer;

        public GoodbyeDPIStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(IsStopped));
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string Log
        {
            get => _log;
            set { _log = value; OnPropertyChanged(nameof(Log)); }
        }

        public bool AutoStartWithRoblox
        {
            get => App.Settings.Prop.UseGoodbyeDPI;
            set
            {
                App.Settings.Prop.UseGoodbyeDPI = value;
                OnPropertyChanged(nameof(AutoStartWithRoblox));
            }
        }

        public bool HasError => Status == GoodbyeDPIStatus.Error;
        public bool IsRunning => Status == GoodbyeDPIStatus.Running;
        public bool IsStopped => Status == GoodbyeDPIStatus.Stopped;
        public bool CanStart => Status == GoodbyeDPIStatus.Stopped || Status == GoodbyeDPIStatus.Error;
        public bool CanStop => Status == GoodbyeDPIStatus.Running;
        public bool IsDownloaded => GoodbyeDPIManager.IsDownloaded;

        public string StatusText => Status switch
        {
            GoodbyeDPIStatus.Stopped => Resources.Strings.Menu_GoodbyeDPI_Status_Stopped,
            GoodbyeDPIStatus.Downloading => Resources.Strings.Menu_GoodbyeDPI_Status_Downloading,
            GoodbyeDPIStatus.Starting => Resources.Strings.Menu_GoodbyeDPI_Status_Starting,
            GoodbyeDPIStatus.Running => Resources.Strings.Menu_GoodbyeDPI_Status_Running,
            GoodbyeDPIStatus.Stopping => Resources.Strings.Menu_GoodbyeDPI_Status_Stopping,
            GoodbyeDPIStatus.Error => Resources.Strings.Menu_GoodbyeDPI_Status_Error,
            _ => "?"
        };

        public string StatusColor => Status switch
        {
            GoodbyeDPIStatus.Running => "#4CAF50",
            GoodbyeDPIStatus.Stopped => "#9E9E9E",
            GoodbyeDPIStatus.Error => "#F44336",
            GoodbyeDPIStatus.Downloading => "#FF9800",
            GoodbyeDPIStatus.Starting => "#FF9800",
            GoodbyeDPIStatus.Stopping => "#FF9800",
            _ => "#9E9E9E"
        };

        public ICommand StartCommand => new AsyncRelayCommand(StartAsync);
        public ICommand StopCommand => new RelayCommand(Stop);
        public ICommand OpenFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", GoodbyeDPIManager.BasePath));
        public ICommand ClearLogCommand => new RelayCommand(() => Log = "");

        public GoodbyeDPIViewModel()
        {
            _autoStartWithRoblox = App.Settings.Prop.UseGoodbyeDPI;
            _status = GoodbyeDPIStatus.Stopped;
            _isDownloaded = GoodbyeDPIManager.IsDownloaded;

            GoodbyeDPIManager.StatusChanged += OnGoodbyeDPIStatusChanged;
            GoodbyeDPIManager.LogReceived += OnGoodbyeDPILogReceived;

            _statusTimer = new Timer(_ =>
            {
                bool running = IsProcessRunning();
                if (running && Status == GoodbyeDPIStatus.Stopped)
                {
                    GoodbyeDPIManager.StopAll();
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        private async Task StartAsync()
        {
            if (!GoodbyeDPIManager.IsDownloaded)
            {
                Status = GoodbyeDPIStatus.Downloading;
                try
                {
                    await GoodbyeDPIManager.EnsureDownloaded();
                    OnPropertyChanged(nameof(IsDownloaded));
                }
                catch
                {
                    return;
                }
            }

            GoodbyeDPIManager.Start();
        }

        private void Stop()
        {
            GoodbyeDPIManager.Stop();
        }

        private void OnGoodbyeDPIStatusChanged(object? sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Status = GoodbyeDPIManager.Status;
            });
        }

        private void OnGoodbyeDPILogReceived(object? sender, string line)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Log += line + "\n";
            });
        }

        private static bool IsProcessRunning()
        {
            try
            {
                return Process.GetProcessesByName("goodbyedpi").Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _statusTimer?.Dispose();
            GoodbyeDPIManager.StatusChanged -= OnGoodbyeDPIStatusChanged;
            GoodbyeDPIManager.LogReceived -= OnGoodbyeDPILogReceived;
        }
    }
}
