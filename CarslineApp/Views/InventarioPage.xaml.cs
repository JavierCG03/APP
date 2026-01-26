using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class InventarioPage : ContentPage
    {
        private readonly InventarioViewModel _viewModel;

        public InventarioPage()
        {
            InitializeComponent();
            _viewModel = new InventarioViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarAsync();
        }
    }
}