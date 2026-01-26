using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class ListaUsuariosPage : ContentPage
    {
        private readonly ListaUsuariosViewModel _viewModel;

        public ListaUsuariosPage()
        {
            InitializeComponent();
            _viewModel = new ListaUsuariosViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Cargar usuarios cuando la pagina aparece
            await _viewModel.CargarUsuarios();
        }
    }
}