using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class CrearOrdenPage : ContentPage
    {
        private readonly CrearOrdenViewModel _viewModel;

        public CrearOrdenPage(int tipoOrdenId)
        {
            InitializeComponent();
            _viewModel = new CrearOrdenViewModel(tipoOrdenId);
            BindingContext = _viewModel;
        }

        // Evento para recalcular costo cuando se selecciona un servicio extra
        private void OnServicioExtraChanged(object sender, CheckedChangedEventArgs e)
        {
            _viewModel.CalcularCostoTotal();
        }
    }
}