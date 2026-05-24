using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fluxstrap.Models.APIs.Roblox;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class ServiceStatus
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "unknown";
        public string Description { get; set; } = "";
        public bool IsOperational => Status == "operational";
        public bool IsUnknown => Status == "unknown";
        public string StatusColor => Status switch
        {
            "operational" => "#4CAF50",
            "degraded_performance" => "#FF9800",
            "partial_outage" => "#F44336",
            "major_outage" => "#D32F2F",
            _ => "#9E9E9E"
        };
        public string StatusLabel => Status switch
        {
            "operational" => Strings.Menu_ServerStatus_StatusOperational,
            "degraded_performance" => Strings.Menu_ServerStatus_StatusDegraded,
            "partial_outage" => Strings.Menu_ServerStatus_StatusPartialOutage,
            "major_outage" => Strings.Menu_ServerStatus_StatusMajorOutage,
            _ => Strings.Menu_ServerStatus_StatusUnknown
        };
    }

    public class ServerStatusViewModel : NotifyPropertyChangedViewModel
    {
        private bool _isLoading = false;
        private string _error = "";
        private string _lastUpdated = "";
        private bool _hasLoaded = false;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); OnPropertyChanged(nameof(ShowContent)); }
        }

        public string Error
        {
            get => _error;
            set { _error = value; OnPropertyChanged(nameof(Error)); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(Error);

        public string LastUpdated
        {
            get => _lastUpdated;
            set { _lastUpdated = value; OnPropertyChanged(nameof(LastUpdated)); }
        }

        public bool ShowContent => !IsLoading && !HasError && _hasLoaded;

        public ObservableCollection<ServiceStatus> Services { get; } = new();

        public ICommand RefreshCommand => new AsyncRelayCommand(LoadAsync);

        public ServerStatusViewModel()
        {
        }

        public async Task LoadAsync()
        {
            const string LOG_IDENT = "ServerStatusViewModel::LoadAsync";

            if (IsLoading)
                return;

            IsLoading = true;
            Error = "";
            Services.Clear();

            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await Http.GetJson<StatuspageResponse>("https://status.roblox.com/api/v2/summary.json");

                if (response?.Components is null || response.Components.Count == 0)
                {
                    Error = Strings.Menu_ServerStatus_ErrorNoData;
                    App.Logger.WriteLine(LOG_IDENT, "Empty response from status API");
                    IsLoading = false;
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, $"Loaded {response.Components.Count} status components");

                foreach (var component in response.Components)
                {
                    Services.Add(new ServiceStatus
                    {
                        Name = component.Name,
                        Status = string.IsNullOrEmpty(component.Status) ? "unknown" : component.Status,
                        Description = component.Description ?? ""
                    });
                }

                if (!string.IsNullOrEmpty(response.Page?.UpdatedAt))
                {
                    try
                    {
                        LastUpdated = DateTime.Parse(response.Page.UpdatedAt).ToString("yyyy-MM-dd HH:mm:ss UTC");
                    }
                    catch
                    {
                        LastUpdated = response.Page.UpdatedAt;
                    }
                }

                _hasLoaded = true;
            }
            catch (TaskCanceledException)
            {
                App.Logger.WriteLine(LOG_IDENT, "Request timed out");
                Error = Strings.Menu_ServerStatus_ErrorTimeout;
            }
            catch (HttpRequestException ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Error = string.Format(Strings.Menu_ServerStatus_ErrorHttp, ex.Message);
            }
            catch (JsonException ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Error = string.Format(Strings.Menu_ServerStatus_ErrorParse, ex.Message);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Error = string.Format(Strings.Menu_ServerStatus_ErrorGeneric, ex.Message);
            }

            OnPropertyChanged(nameof(Services));
            IsLoading = false;
        }
    }
}
