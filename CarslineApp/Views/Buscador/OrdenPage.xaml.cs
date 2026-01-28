using CarslineApp.ViewModels.ViewModelBuscador;

namespace CarslineApp.Views.Buscador;

public partial class OrdenPage : ContentPage
{
    public OrdenPage(int ordenId)
    {
        InitializeComponent();
        BindingContext = new OrdenDetalleViewModel(ordenId);
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
    private void OnGenerarPdfDebugClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("??? BOTÓN FÍSICO PRESIONADO!");
        System.Diagnostics.Debug.WriteLine($"   - BindingContext: {BindingContext?.GetType().Name ?? "NULL"}");

        var vm = BindingContext as OrdenDetalleViewModel;
        if (vm != null)
        {
            System.Diagnostics.Debug.WriteLine($"   - ViewModel: OK");
            System.Diagnostics.Debug.WriteLine($"   - TieneOrden: {vm.TieneOrden}");
            System.Diagnostics.Debug.WriteLine($"   - Orden: {(vm.Orden == null ? "NULL" : vm.Orden.NumeroOrden)}");
            System.Diagnostics.Debug.WriteLine($"   - GenerarPdfCommand: {(vm.GenerarPdfCommand == null ? "NULL" : "OK")}");

            if (vm.GenerarPdfCommand != null && vm.GenerarPdfCommand.CanExecute(null))
            {
                System.Diagnostics.Debug.WriteLine("   - Ejecutando comando manualmente...");
                vm.GenerarPdfCommand.Execute(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("   - ?? Comando no puede ejecutarse");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("   - ? ViewModel es NULL!");
        }
    }
}