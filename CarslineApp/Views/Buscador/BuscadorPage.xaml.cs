using CarslineApp.ViewModels;

namespace CarslineApp.Views.Buscador; 

public partial class BuscadorPage : ContentPage
{
    public BuscadorPage()
    {
        InitializeComponent();
        BindingContext = new BuscadorViewModel();
    }
}