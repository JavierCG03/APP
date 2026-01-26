using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class CheckListServicioPage : ContentPage
    {
        private readonly CheckListServicioViewModel _viewModel;
        private string _valorBalatasDelanteras = string.Empty;
        private string _valorBalatasTraseras = string.Empty;

        public CheckListServicioPage(int trabajoId, int ordenId, string orden, string trabajo, string vehiculo, string Indicaciones, string VIN)
        {
            InitializeComponent();
            _viewModel = new CheckListServicioViewModel(trabajoId, ordenId, orden, trabajo, vehiculo, Indicaciones, VIN);
            BindingContext = _viewModel;
        }

        private void Radio_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _viewModel.SetValor(radio.GroupName, radio.Value);
        }

        // ? NUEVO: Evento específico para Balatas Delanteras
        private void BalatasDelanteras_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _valorBalatasDelanteras = radio.Value?.ToString() ?? string.Empty;

            // Guardar valor en el CheckList
            _viewModel.SetValor(radio.GroupName, radio.Value);

            // Validar si mostrar evidencia
            _viewModel.ValidarMostrarEvidenciaBalatasDelanteras(_valorBalatasDelanteras);
        }

        // ? NUEVO: Evento específico para Balatas Traseras
        private void BalatasTraseras_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _valorBalatasTraseras = radio.Value?.ToString() ?? string.Empty;

            // Guardar valor en el CheckList
            _viewModel.SetValor(radio.GroupName, radio.Value);

            // Validar si mostrar evidencia
            _viewModel.ValidarMostrarEvidenciaBalatasTraseras(_valorBalatasTraseras);
        }

        // ? NUEVO: Evento para Reemplazo de Aceite Motor
        private void ReemplazoAceiteMotor_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _viewModel.SetValor(radio.GroupName, radio.Value);

            // La visibilidad ya se controla automáticamente en el ViewModel
        }

        // ? NUEVO: Evento para Reemplazo de Filtro de Aceite
        private void ReemplazoFiltroAceite_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _viewModel.SetValor(radio.GroupName, radio.Value);
        }

        // ? NUEVO: Evento para Reemplazo de Filtro de Aire Motor
        private void ReemplazoFiltroAireMotor_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _viewModel.SetValor(radio.GroupName, radio.Value);
        }

        // ? NUEVO: Evento para Reemplazo de Filtro de Aire Polen
        private void ReemplazoFiltroAirePolen_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            _viewModel.SetValor(radio.GroupName, radio.Value);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _viewModel.RestablecerEstadoTrabajo();
        }
    }
}