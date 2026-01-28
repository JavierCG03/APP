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
        ConfigurarBarraNavegacion();
    }
    private void ConfigurarBarraNavegacion()
    {

        Shell.SetBackgroundColor(this, Color.FromArgb("#D60000"));
        Shell.SetForegroundColor(this, Colors.White);


        if (Application.Current?.MainPage is NavigationPage navigationPage)
        {
            navigationPage.BarBackgroundColor = Color.FromArgb("#D60000");
            navigationPage.BarTextColor = Colors.White;
        }
    }
}

