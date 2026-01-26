using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class CrearUsuarioPage : ContentPage
    {
        public CrearUsuarioPage()
        {
            InitializeComponent();
            BindingContext = new CrearUsuarioViewModel();
        }
    }
}