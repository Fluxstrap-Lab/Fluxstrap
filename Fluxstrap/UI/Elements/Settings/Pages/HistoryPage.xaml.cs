using Fluxstrap.UI.ViewModels.Pages;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class HistoryPage
    {
        private readonly HistoryPageViewModel _viewModel;
        public HistoryPage()
        {
            InitializeComponent();
            _viewModel = new HistoryPageViewModel();
            DataContext = _viewModel;
        }
    }
}
