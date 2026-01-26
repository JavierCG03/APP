using CarslineApp.ViewModels.ViewModelBuscador;
namespace CarslineApp.Views.Buscador;

    public partial class ClientesPage : ContentPage
    {
        public ClientesPage(int clienteId)
        {
            InitializeComponent();
            BindingContext = new ClienteDetalleViewModel(clienteId);

        }
    }
