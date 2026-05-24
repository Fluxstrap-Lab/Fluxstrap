using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    public partial class StatisticsPage
    {
        public StatisticsPage()
        {
            DataContext = new StatisticsViewModel();
            InitializeComponent();
        }
    }
}
