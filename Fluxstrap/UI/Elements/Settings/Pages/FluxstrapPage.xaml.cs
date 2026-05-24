using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxstrap.UI.ViewModels.Settings;

namespace Fluxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FluxstrapPage.xaml
    /// </summary>
    public partial class FluxstrapPage
    {
        public FluxstrapPage()
        {
            DataContext = new FluxstrapViewModel();
            InitializeComponent();
        }
    }
}
