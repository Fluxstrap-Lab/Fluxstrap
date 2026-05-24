using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Fluxstrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        private readonly IThemeService _themeService = new ThemeService();

        private static readonly Color GoldAccent = Color.FromRgb(0xC0, 0x91, 0x04);
        private static readonly Color GoldAccentLight = Color.FromRgb(0xD4, 0xA8, 0x14);
        private static readonly Color GoldAccentDark = Color.FromRgb(0xA6, 0x7A, 0x00);

        public WpfUiWindow()
        {
            ApplyTheme();
        }

        public static void ApplyThemeResources()
        {
            Application.Current.Resources["SystemAccentColorPrimary"] = GoldAccent;
            Application.Current.Resources["SystemAccentColorSecondary"] = GoldAccent;
            Application.Current.Resources["SystemAccentColorTertiary"] = GoldAccentLight;

            Application.Current.Resources["SystemAccentColorPrimaryBrush"] = new SolidColorBrush(GoldAccent);
            Application.Current.Resources["SystemAccentColorSecondaryBrush"] = new SolidColorBrush(GoldAccent);
            Application.Current.Resources["SystemAccentColorTertiaryBrush"] = new SolidColorBrush(GoldAccentLight);
        }

        public void ApplyTheme()
        {
            const int customThemeIndex = 2; // index for CustomTheme merged dictionary

            _themeService.SetTheme(App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);

            ApplyThemeResources();

            // there doesn't seem to be a way to query the name for merged dictionaries
            var dict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/UI/Style/{Enum.GetName(App.Settings.Prop.Theme.GetFinal())}.xaml") };
            Application.Current.Resources.MergedDictionaries[customThemeIndex] = dict;

#if QA_BUILD
            this.BorderBrush = System.Windows.Media.Brushes.Red;
            this.BorderThickness = new Thickness(4);
#endif
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            base.OnSourceInitialized(e);
        }
    }
}
