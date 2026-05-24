using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fluxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new System.Drawing.Size(width, height));

        public static ImageSource? GetImageSource(this Icon icon, bool handleException = true)
        {
            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                if (!handleException)
                    throw;

                try
                {
                    using MemoryStream stream = new();
                    icon.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("IconEx::GetImageSource", ex);

                    try
                    {
                        return CreateFallbackImageSource();
                    }
                    catch (Exception ex2)
                    {
                        App.Logger.WriteException("IconEx::GetImageSource (fallback)", ex2);
                        return null;
                    }
                }
            }
        }

        private static ImageSource CreateFallbackImageSource()
        {
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing
            {
                Geometry = new RectangleGeometry(new Rect(0, 0, 32, 32)),
                Brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC0, 0x91, 0x04))
            });
            return new DrawingImage(drawingGroup);
        }
    }
}
