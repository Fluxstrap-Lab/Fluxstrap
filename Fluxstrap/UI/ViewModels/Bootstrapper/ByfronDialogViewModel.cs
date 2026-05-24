using System;
using System.Windows;
using System.Windows.Media;

namespace Fluxstrap.UI.ViewModels.Bootstrapper
{
    public class ByfronDialogViewModel : BootstrapperDialogViewModel
    {
        private ImageSource? _byfronLogoLocation;

        public ImageSource ByfronLogoLocation
        {
            get => _byfronLogoLocation ??= LoadLogo("Dark");
            set => _byfronLogoLocation = value;
        }

        public Thickness DialogBorder { get; set; } = new Thickness(0);
        public Brush Background { get; set; } = Brushes.Black;
        public Brush Foreground { get; set; } = new SolidColorBrush(Color.FromRgb(239, 239, 239));
        public Brush IconColor { get; set; } = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public Brush ProgressBarBackground { get; set; } = new SolidColorBrush(Color.FromRgb(86, 86, 86));

        public Visibility VersionTextVisibility => CancelEnabled ? Visibility.Collapsed : Visibility.Visible;

        public string VersionText { get; init; }

        public ByfronDialogViewModel(IBootstrapperDialog dialog, string version) : base(dialog)
        {
            VersionText = version;
        }

        private static ImageSource LoadLogo(string theme)
        {
            try
            {
                return new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/Resources/BootstrapperStyles/ByfronDialog/ByfronLogo{theme}.jpg"));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ByfronDialogViewModel::LoadLogo", ex);
                var group = new DrawingGroup();
                group.Children.Add(new GeometryDrawing
                {
                    Geometry = new EllipseGeometry(new System.Windows.Point(57, 54), 50, 50),
                    Brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200))
                });
                return new DrawingImage(group);
    }
}
    }
}
