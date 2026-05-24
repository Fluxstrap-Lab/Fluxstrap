using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class ServerStatusPage
    {
        public ServerStatusPage()
        {
            DataContext = new ServerStatusViewModel();
            InitializeComponent();
        }
    }
}
