using CarslineApp.ViewModels.ViewModelBuscador;
namespace CarslineApp.Views.Buscador;

public partial class ClientesPage : ContentPage
{
    public ClientesPage(int clienteId)
    {
        InitializeComponent();
        BindingContext = new ClienteDetalleViewModel(clienteId);

        // Configurar color de la barra de navegación a rojo
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