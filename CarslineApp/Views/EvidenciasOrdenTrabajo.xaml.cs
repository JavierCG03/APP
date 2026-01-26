using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class EvidenciasOrdenTrabajo : ContentPage
    {
        private EvidenciaOrdenTrabajoViewModel ViewModel => BindingContext as EvidenciaOrdenTrabajoViewModel;

        public EvidenciasOrdenTrabajo(int ordenGeneralId, int tipoevidenica)
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine($"?? Constructor de EvidenciasOrdenTrabajo - OrdenId: {ordenGeneralId}");

            if (BindingContext is EvidenciaOrdenTrabajoViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"? ViewModel encontrado, asignando OrdenGeneralId: {ordenGeneralId}");
                viewModel.OrdenGeneralId = ordenGeneralId;
                viewModel.TipoEvidencia = tipoevidenica;

                // Asignar referencia al ScrollView para la navegación
                viewModel.SetScrollView(CarouselScrollView);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"? ERROR: BindingContext no es EvidenciaOrdenTrabajoViewModel");
            }
        }

        // Evento para detectar cambios en el scroll y actualizar la visibilidad de los botones
        private void OnCarouselScrolled(object sender, ScrolledEventArgs e)
        {
            ViewModel?.ActualizarEstadoBotones(e.ScrollX);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("??? EvidenciasOrdenTrabajo - OnAppearing");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("?? EvidenciasOrdenTrabajo - OnDisappearing");
        }
    }
}