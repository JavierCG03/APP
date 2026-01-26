using CarslineApp.ViewModels.ViewModelBuscador;

namespace CarslineApp.Views.Buscador;

public partial class OrdenPage : ContentPage
{
    public OrdenPage(int ordenId)
    {
        InitializeComponent();
        BindingContext = new OrdenDetalleViewModel(ordenId);
    }
}