using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class GoodbyeDPIPage
    {
        public GoodbyeDPIViewModel ViewModel { get; }

        public GoodbyeDPIPage()
        {
            ViewModel = new GoodbyeDPIViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
