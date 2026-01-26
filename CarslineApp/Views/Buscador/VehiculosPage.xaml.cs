using CarslineApp.ViewModels.ViewModelBuscador;
namespace CarslineApp.Views.Buscador;

public partial class VehiculosPage : ContentPage
{

    public VehiculosPage(int vehiculoId)
    {
        {
            InitializeComponent();
            BindingContext = new VehiculoDetalleViewModel(vehiculoId);
        }
    }
}
