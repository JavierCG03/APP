using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class EvidenciasOrdenPage : ContentPage
    {
        public EvidenciasOrdenPage(int ordenGeneralId)
        {
            InitializeComponent();

            if (BindingContext is EvidenciaOrdenViewModel viewModel)
            {
                viewModel.OrdenGeneralId = ordenGeneralId;
            }
        }

        public EvidenciasOrdenPage()
        {
            InitializeComponent();
        }
    }
}