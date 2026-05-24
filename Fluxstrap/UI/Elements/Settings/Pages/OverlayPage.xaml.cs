using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class OverlayPage
    {
        public OverlayPage()
        {
            DataContext = new OverlayViewModel();
            InitializeComponent();
        }
    }
}
