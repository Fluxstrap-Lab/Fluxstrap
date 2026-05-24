using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class SystemInfoPage
    {
        public SystemInfoViewModel ViewModel { get; }

        public SystemInfoPage()
        {
            ViewModel = new SystemInfoViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
