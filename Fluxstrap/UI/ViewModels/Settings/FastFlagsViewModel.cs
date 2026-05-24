using System.Text.Json;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using Fluxstrap.Enums.FlagPresets;
using Microsoft.Win32;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class FastFlagsViewModel : NotifyPropertyChangedViewModel
    {
        private Dictionary<string, object>? _preResetFlags;

        public event EventHandler? RequestPageReloadEvent;
        
        public event EventHandler? OpenFlagEditorEvent;

        private void OpenFastFlagEditor() => OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);

        public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

        public Visibility CanShowFastFlagEditor => App.IsStudioInstalled ? Visibility.Visible : Visibility.Collapsed;

        public bool UseFastFlagManager
        {
            get => App.Settings.Prop.UseFastFlagManager;
            set => App.Settings.Prop.UseFastFlagManager = value;
        }

        public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

        public MSAAMode SelectedMSAALevel
        {
            get => MSAALevels.FirstOrDefault(x => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
            set => App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
        }

        public bool FixDisplayScaling
        {
            get => App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
            set => App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
        }

        public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

        public TextureQuality SelectedTextureQuality
        {
            get => TextureQualities.Where(x => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
            set
            {
                if (value == TextureQuality.Default)
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality", null);
                }
                else
                {
                    App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
                }
            }
        }

        public IReadOnlyList<FastFlagPreset> FastFlagPresets => (FastFlagPreset[])Enum.GetValues(typeof(FastFlagPreset));

        public FastFlagPreset SelectedFastFlagPreset
        {
            get => App.Settings.Prop.FastFlagPreset;
            set
            {
                if (value == App.Settings.Prop.FastFlagPreset)
                    return;

                App.Settings.Prop.FastFlagPreset = value;
                App.Settings.Save();
                App.FastFlags.ApplyPreset(value);
                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool EnableDevConsole
        {
            get => App.Settings.Prop.EnableDevConsole;
            set
            {
                App.Settings.Prop.EnableDevConsole = value;
                App.FastFlags.SetValue("FFlagDebugDisableF9", value ? null : "True");
                App.FastFlags.Save();
            }
        }

        public bool ResetConfiguration
        {
            get => _preResetFlags is not null;

            set
            {
                if (value)
                {
                    _preResetFlags = new(App.FastFlags.Prop);
                    App.FastFlags.Prop.Clear();
                }
                else
                {
                    App.FastFlags.Prop = _preResetFlags!;
                    _preResetFlags = null;
                }

                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public ICommand ExportFastFlagsCommand => new RelayCommand(() =>
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                DefaultExt = ".json",
                FileName = "ClientAppSettings.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                string json = JsonSerializer.Serialize(App.FastFlags.Prop, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FastFlagsViewModel::ExportFastFlags", ex);
            }
        });

        public ICommand ImportFastFlagsCommand => new RelayCommand(() =>
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                var imported = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (imported is null) return;

                foreach (var pair in imported)
                    App.FastFlags.Prop[pair.Key] = pair.Value;

                App.FastFlags.Save();
                RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FastFlagsViewModel::ImportFastFlags", ex);
            }
        });
    }
}
