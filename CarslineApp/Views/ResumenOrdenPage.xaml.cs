using CarslineApp.Models;
using CarslineApp.ViewModels.Creacion_Ordenes;

namespace CarslineApp.Views
{
    public partial class ResumenOrdenPage : ContentPage
    {
        public ResumenOrdenPage(
            int tipoOrdenId,
            int clienteId,
            int vehiculoId,
            int tipoServicioId,
            int kilometrajeActual,
            DateTime fechaHoraPromesa,
            string observaciones,
            List<TrabajoCrearDto> trabajos)
        {
            InitializeComponent();

            var viewModel = new ResumenOrdenViewModel(
                tipoOrdenId,
                clienteId,
                vehiculoId,
                tipoServicioId,
                kilometrajeActual,
                fechaHoraPromesa,
                observaciones,
                trabajos
            );

            BindingContext = viewModel;
        }
    }
}