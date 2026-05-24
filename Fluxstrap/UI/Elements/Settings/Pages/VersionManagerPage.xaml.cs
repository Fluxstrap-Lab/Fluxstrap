using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class VersionManagerPage
    {
        public VersionManagerPage()
        {
            DataContext = new VersionManagerViewModel();
            InitializeComponent();
        }
    }
}
