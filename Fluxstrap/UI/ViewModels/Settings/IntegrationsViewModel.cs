using System.Collections.ObjectModel;
using System.Windows.Input;

using Microsoft.Win32;

using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class IntegrationsViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand AddIntegrationCommand => new RelayCommand(AddIntegration);

        public ICommand DeleteIntegrationCommand => new RelayCommand(DeleteIntegration);

        public ICommand BrowseIntegrationLocationCommand => new RelayCommand(BrowseIntegrationLocation);

        private void AddIntegration()
        {
            CustomIntegrations.Add(new CustomIntegration()
            {
                Name = Strings.Menu_Integrations_Custom_NewIntegration
            });

            SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;

            OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        private void DeleteIntegration()
        {
            if (SelectedCustomIntegration is null)
                return;

            CustomIntegrations.Remove(SelectedCustomIntegration);

            if (CustomIntegrations.Count > 0)
            {
                SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
                OnPropertyChanged(nameof(SelectedCustomIntegrationIndex));
            }

            OnPropertyChanged(nameof(IsCustomIntegrationSelected));
        }

        private void BrowseIntegrationLocation()
        {
            if (SelectedCustomIntegration is null)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.Menu_AllFiles}|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            SelectedCustomIntegration.Name = dialog.SafeFileName;
            SelectedCustomIntegration.Location = dialog.FileName;
            OnPropertyChanged(nameof(SelectedCustomIntegration));
        }

        public bool ActivityTrackingEnabled
        {
            get => App.Settings.Prop.EnableActivityTracking;
            set
            {
                App.Settings.Prop.EnableActivityTracking = value;

                if (!value)
                {
                    ShowServerDetailsEnabled = value;
                    DisableAppPatchEnabled = value;
                    DiscordActivityEnabled = value;
                    DiscordActivityJoinEnabled = value;

                    OnPropertyChanged(nameof(ShowServerDetailsEnabled));
                    OnPropertyChanged(nameof(DisableAppPatchEnabled));
                    OnPropertyChanged(nameof(DiscordActivityEnabled));
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                }
            }
        }

        public bool ShowServerDetailsEnabled
        {
            get => App.Settings.Prop.ShowServerDetails;
            set => App.Settings.Prop.ShowServerDetails = value;
        }

        public bool DiscordActivityEnabled
        {
            get => App.Settings.Prop.UseDiscordRichPresence;
            set
            {
                App.Settings.Prop.UseDiscordRichPresence = value;

                if (!value)
                {
                    DiscordActivityJoinEnabled = value;
                    DiscordAccountOnProfile = value;
                    OnPropertyChanged(nameof(DiscordActivityJoinEnabled));
                    OnPropertyChanged(nameof(DiscordAccountOnProfile));
                }
            }
        }

        public bool DiscordActivityJoinEnabled
        {
            get => !App.Settings.Prop.HideRPCButtons;
            set => App.Settings.Prop.HideRPCButtons = !value;
        }

        public bool DiscordAccountOnProfile
        {
            get => App.Settings.Prop.ShowAccountOnRichPresence;
            set => App.Settings.Prop.ShowAccountOnRichPresence = value;
        }

        public bool DisableAppPatchEnabled
        {
            get => App.Settings.Prop.UseDisableAppPatch;
            set => App.Settings.Prop.UseDisableAppPatch = value;
        }

        public bool GoodbyeDPIEnabled
        {
            get => App.Settings.Prop.UseGoodbyeDPI;
            set
            {
                App.Settings.Prop.UseGoodbyeDPI = value;
                if (value)
                    _ = GoodbyeDPIManager.EnsureDownloaded();
            }
        }

        public ObservableCollection<CustomIntegration> CustomIntegrations
        {
            get => App.Settings.Prop.CustomIntegrations;
            set => App.Settings.Prop.CustomIntegrations = value;
        }

        public CustomIntegration? SelectedCustomIntegration { get; set; }
        public int SelectedCustomIntegrationIndex { get; set; }
        public bool IsCustomIntegrationSelected => SelectedCustomIntegration is not null;

        // Server Region Blacklist

        public bool EnableServerBlacklist
        {
            get => App.Settings.Prop.EnableServerBlacklist;
            set
            {
                App.Settings.Prop.EnableServerBlacklist = value;
                OnPropertyChanged(nameof(EnableServerBlacklist));
            }
        }

        public ObservableCollection<string> BlockedServerRegions => App.Settings.Prop.BlockedServerRegions;

        public string? NewBlockedRegion { get; set; }

        public ICommand AddBlockedRegionCommand => new RelayCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(NewBlockedRegion))
                return;

            string code = NewBlockedRegion.Trim().ToUpperInvariant();
            if (code.Length != 2)
                return;

            if (!BlockedServerRegions.Contains(code))
            {
                BlockedServerRegions.Add(code);
                OnPropertyChanged(nameof(BlockedServerRegions));
            }

            NewBlockedRegion = "";
            OnPropertyChanged(nameof(NewBlockedRegion));
        });

        public ICommand RemoveBlockedRegionCommand => new RelayCommand<string>(code =>
        {
            if (!string.IsNullOrEmpty(code) && BlockedServerRegions.Contains(code))
            {
                BlockedServerRegions.Remove(code);
                OnPropertyChanged(nameof(BlockedServerRegions));
            }
        });
    }
}
