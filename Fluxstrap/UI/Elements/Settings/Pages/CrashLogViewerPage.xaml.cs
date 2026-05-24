using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class CrashLogViewerPage
    {
        public CrashLogViewerViewModel ViewModel { get; }

        public CrashLogViewerPage()
        {
            ViewModel = new CrashLogViewerViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
