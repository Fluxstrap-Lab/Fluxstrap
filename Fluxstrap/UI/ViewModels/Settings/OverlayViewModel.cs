using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fluxstrap.Resources;
using Fluxstrap.Utility;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class OverlayViewModel : NotifyPropertyChangedViewModel
    {
        public bool OverlayEnabled
        {
            get => App.Settings.Prop.OverlayEnabled;
            set
            {
                App.Settings.Prop.OverlayEnabled = value;
                OnPropertyChanged(nameof(OverlayEnabled));
                OnPropertyChanged(nameof(OverlayStatusText));
            }
        }

        public bool OverlayShowFps
        {
            get => App.Settings.Prop.OverlayShowFps;
            set => App.Settings.Prop.OverlayShowFps = value;
        }

        public bool OverlayShowPing
        {
            get => App.Settings.Prop.OverlayShowPing;
            set => App.Settings.Prop.OverlayShowPing = value;
        }

        public bool OverlayShowRam
        {
            get => App.Settings.Prop.OverlayShowRam;
            set => App.Settings.Prop.OverlayShowRam = value;
        }

        public bool OverlayShowServer
        {
            get => App.Settings.Prop.OverlayShowServer;
            set => App.Settings.Prop.OverlayShowServer = value;
        }

        public bool OverlayShowFriends
        {
            get => App.Settings.Prop.OverlayShowFriends;
            set => App.Settings.Prop.OverlayShowFriends = value;
        }

        public bool OverlayShowCpu
        {
            get => App.Settings.Prop.OverlayShowCpu;
            set => App.Settings.Prop.OverlayShowCpu = value;
        }

        public bool OverlayShowGpu
        {
            get => App.Settings.Prop.OverlayShowGpu;
            set => App.Settings.Prop.OverlayShowGpu = value;
        }

        public bool OverlayShowVolume
        {
            get => App.Settings.Prop.OverlayShowVolume;
            set => App.Settings.Prop.OverlayShowVolume = value;
        }

        public bool OverlayShowGraph
        {
            get => App.Settings.Prop.OverlayShowGraph;
            set => App.Settings.Prop.OverlayShowGraph = value;
        }

        public string OverlayPosition
        {
            get => App.Settings.Prop.OverlayPosition;
            set => App.Settings.Prop.OverlayPosition = value;
        }

        public double OverlayOpacity
        {
            get => App.Settings.Prop.OverlayOpacity;
            set => App.Settings.Prop.OverlayOpacity = value;
        }

        public string OverlayToggleHotkey
        {
            get => App.Settings.Prop.OverlayToggleHotkey;
            set => App.Settings.Prop.OverlayToggleHotkey = value;
        }

        public string OverlayAccentColor
        {
            get => App.Settings.Prop.OverlayAccentColor;
            set
            {
                App.Settings.Prop.OverlayAccentColor = value;
                OnPropertyChanged(nameof(OverlayAccentColor));
            }
        }

        public string OverlayStatusText => OverlayEnabled ? "Enabled" : "Disabled";

        public string[] PositionOptions => new[] { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        public string[] AccentColorOptions => new[]
        {
            "#00A2FF", "#7A39FB", "#00E676", "#FFD600",
            "#FF5252", "#FF6D00", "#E040FB", "#00BCD4"
        };

        public ICommand ToggleOverlayNowCommand => new RelayCommand(ToggleOverlayNow);
        public ICommand ResetOverlayPositionCommand => new RelayCommand(ResetPosition);

        private void ToggleOverlayNow()
        {
            GameOverlayManager.ToggleVisibility();
        }

        private void ResetPosition()
        {
            OverlayPosition = "TopRight";
        }
    }
}
