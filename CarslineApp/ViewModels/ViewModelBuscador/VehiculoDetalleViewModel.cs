using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Views.Buscador;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.ViewModelBuscador
{
    public class VehiculoDetalleViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _vehiculoId;

        private VehiculoDto _vehiculo;
        private ClienteDto _cliente;
        private ObservableCollection<OrdenSimpleDto> _ordenesVehiculo;
        private bool _isLoading;
        private bool _modoEdicionPlacas;
        private string _nuevasPlacas;
        private string _errorMessage;

        public VehiculoDetalleViewModel(int vehiculoId)
        {
            _apiService = new ApiService();
            _vehiculoId = vehiculoId;
            OrdenesVehiculo = new ObservableCollection<OrdenSimpleDto>();

            // Comandos existentes
            EditarPlacasCommand = new Command(HabilitarEdicionPlacas);
            GuardarPlacasCommand = new Command(async () => await GuardarPlacas());
            CancelarEdicionCommand = new Command(CancelarEdicion);
            VerClienteCommand = new Command(async () => await VerCliente());
            VerOrdenCommand = new Command<int>(async (ordenId) => await VerOrden(ordenId));

            // Nuevos comandos para accesos directos
            CopiarVINCommand = new Command(async () => await CopiarVIN());
            CopiarPlacasCommand = new Command(async () => await CopiarPlacas());
            ConsultarREPUVEPorVINCommand = new Command(async () => await ConsultarREPUVEPorVIN());
            LlamarPropietarioCommand = new Command(async () => await LlamarPropietario());

            CargarDatosVehiculo();
        }

        #region Propiedades

        public VehiculoDto Vehiculo
        {
            get => _vehiculo;
            set { _vehiculo = value; OnPropertyChanged(); }
        }

        public ClienteDto Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OrdenSimpleDto> OrdenesVehiculo
        {
            get => _ordenesVehiculo;
            set { _ordenesVehiculo = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool ModoEdicionPlacas
        {
            get => _modoEdicionPlacas;
            set
            {
                _modoEdicionPlacas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarEdicionPlacas));
                OnPropertyChanged(nameof(MostrarPlacasNormales));
            }
        }

        public string NuevasPlacas
        {
            get => _nuevasPlacas;
            set { _nuevasPlacas = value?.ToUpper(); OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool MostrarEdicionPlacas => ModoEdicionPlacas;
        public bool MostrarPlacasNormales => !ModoEdicionPlacas;
        public bool TieneOrdenes => OrdenesVehiculo?.Any() ?? false;
        public bool TienePlacas => !string.IsNullOrWhiteSpace(Vehiculo?.Placas);

        #endregion

        #region Comandos

        // Comandos existentes
        public ICommand EditarPlacasCommand { get; }
        public ICommand GuardarPlacasCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand VerClienteCommand { get; }
        public ICommand VerOrdenCommand { get; }

        // Nuevos comandos
        public ICommand CopiarVINCommand { get; }
        public ICommand CopiarPlacasCommand { get; }
        public ICommand ConsultarREPUVEPorVINCommand { get; }
        public ICommand LlamarPropietarioCommand { get; }

        #endregion

        #region Métodos de Accesos Directos

        /// <summary>
        /// Copia el VIN al portapapeles
        /// </summary>
        private async Task CopiarVIN()
        {
            if (string.IsNullOrWhiteSpace(Vehiculo?.VIN))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay VIN disponible para copiar",
                    "OK");
                return;
            }

            try
            {
                await Clipboard.SetTextAsync(Vehiculo.VIN);

                await Application.Current.MainPage.DisplayAlert(
                    "✅ Copiado",
                    $"VIN copiado al portapapeles:\n{Vehiculo.VIN}",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo copiar el VIN: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Copia las placas al portapapeles
        /// </summary>
        private async Task CopiarPlacas()
        {
            if (string.IsNullOrWhiteSpace(Vehiculo?.Placas))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay placas disponibles para copiar",
                    "OK");
                return;
            }

            try
            {
                await Clipboard.SetTextAsync(Vehiculo.Placas);

                await Application.Current.MainPage.DisplayAlert(
                    "✅ Copiado",
                    $"Placas copiadas al portapapeles:\n{Vehiculo.Placas}",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo copiar las placas: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Consulta REPUVE usando el VIN
        /// </summary>
        private async Task ConsultarREPUVEPorVIN()
        {
            if (string.IsNullOrWhiteSpace(Vehiculo?.VIN))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay VIN disponible para consultar",
                    "OK");
                return;
            }

            try
            {
                await Clipboard.SetTextAsync(Vehiculo.VIN);
                var repuveUrl = "https://www2.repuve.gob.mx:8443/ciudadania/";

                await Launcher.OpenAsync(new Uri(repuveUrl));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo abrir REPUVE: {ex.Message}",
                    "OK");
            }
        }


        /// <summary>
        /// Llama al propietario del vehículo
        /// </summary>
        private async Task LlamarPropietario()
        {
            if (string.IsNullOrWhiteSpace(Cliente?.TelefonoMovil))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay teléfono disponible del propietario",
                    "OK");
                return;
            }

            try
            {
                var telefonoLimpio = LimpiarNumeroTelefono(Cliente.TelefonoMovil);

                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(telefonoLimpio);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ No disponible",
                        "Este dispositivo no soporta llamadas telefónicas",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo realizar la llamada: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Limpia un número de teléfono
        /// </summary>
        private string LimpiarNumeroTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            var limpio = new string(telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());

            if (limpio.Contains('+'))
            {
                limpio = "+" + limpio.Replace("+", "");
            }

            return limpio;
        }

        #endregion

        #region Métodos Existentes

        private async void CargarDatosVehiculo()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Cargar datos del vehículo
                var vehiculoResponse = await _apiService.ObtenerVehiculoPorIdAsync(_vehiculoId);
                if (vehiculoResponse.Success && vehiculoResponse.Vehiculo != null)
                {
                    Vehiculo = vehiculoResponse.Vehiculo;
                    NuevasPlacas = Vehiculo.Placas;

                    // Cargar datos del cliente
                    var clienteResponse = await _apiService.ObtenerClientePorIdAsync(Vehiculo.ClienteId);
                    if (clienteResponse.Success && clienteResponse.Cliente != null)
                    {
                        Cliente = clienteResponse.Cliente;
                    }

                    // Cargar historial de órdenes
                    await CargarHistorialOrdenes();

                    // Actualizar propiedades calculadas
                    OnPropertyChanged(nameof(TienePlacas));
                }
                else
                {
                    ErrorMessage = vehiculoResponse.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CargarHistorialOrdenes()
        {
            try
            {
                var historialResponse = await _apiService.ObtenerHistorialGeneralVehiculoAsync(_vehiculoId);

                if (historialResponse.Success && historialResponse.Historial != null)
                {
                    OrdenesVehiculo.Clear();

                    foreach (var orden in historialResponse.Historial)
                    {
                        OrdenesVehiculo.Add(new OrdenSimpleDto
                        {
                            Id = orden.ordenId,
                            NumeroOrden = orden.NumeroOrden,
                            FechaCreacion = orden.FechaOrden,
                            ClienteNombre = Cliente?.NombreCompleto ?? "",
                            VehiculoInfo = Vehiculo?.VehiculoCompleto ?? "",
                            EstadoOrden = orden.EstadoOrden
                        });
                    }

                    OnPropertyChanged(nameof(TieneOrdenes));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar órdenes: {ex.Message}");
            }
        }

        private void HabilitarEdicionPlacas()
        {
            ModoEdicionPlacas = true;
        }

        private async Task GuardarPlacas()
        {
            if (string.IsNullOrWhiteSpace(NuevasPlacas))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "Las placas no pueden estar vacías",
                    "OK");
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.ActualizarPlacasVehiculoAsync(_vehiculoId, NuevasPlacas);

                if (response.Success)
                {
          
                    ModoEdicionPlacas = false;
                    Vehiculo.Placas = NuevasPlacas;
                    OnPropertyChanged(nameof(Vehiculo));
                    OnPropertyChanged(nameof(TienePlacas));
                    

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Placas actualizadas correctamente",
                        "OK");
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelarEdicion()
        {
            NuevasPlacas = Vehiculo.Placas;
            ModoEdicionPlacas = false;
        }

        private async Task VerCliente()
        {
            if (Cliente == null) return;

            var clientePage = new ClientesPage(Cliente.Id);
            await Application.Current.MainPage.Navigation.PushAsync(clientePage);
        }

        private async Task VerOrden(int ordenId)
        {
            var ordenPage = new OrdenPage(ordenId);
            await Application.Current.MainPage.Navigation.PushAsync(ordenPage);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}