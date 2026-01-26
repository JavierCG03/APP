using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Views;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Creacion_Ordenes
{
    public class ResumenOrdenViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Datos pasados desde CrearOrdenViewModel
        public int TipoOrdenId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int TipoServicioId { get; set; }
        public int KilometrajeActual { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string ObservacionesAsesor { get; set; }
        public List<TrabajoCrearDto> Trabajos { get; set; }

        // Datos para mostrar en UI
        private string _nombreCliente = string.Empty;
        private string _direccionCliente = string.Empty;
        private string _rfcCliente = string.Empty;
        private string _telefonoCliente = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vinVehiculo = string.Empty;
        private string _placasVehiculo = string.Empty;
        private string _tipoOrdenNombre = string.Empty;
        private string _tipoServicioNombre = string.Empty;
        private decimal _costoTotal;

        public ResumenOrdenViewModel(
            int tipoOrdenId,
            int clienteId,
            int vehiculoId,
            int tipoServicioId,
            int kilometrajeActual,
            DateTime fechaHoraPromesa,
            string observaciones,
            List<TrabajoCrearDto> trabajos)
        {
            _apiService = new ApiService();

            TipoOrdenId = tipoOrdenId;
            ClienteId = clienteId;
            VehiculoId = vehiculoId;
            TipoServicioId = tipoServicioId;
            KilometrajeActual = kilometrajeActual;
            FechaHoraPromesaEntrega = fechaHoraPromesa;
            ObservacionesAsesor = observaciones ?? string.Empty;
            Trabajos = trabajos ?? new List<TrabajoCrearDto>();

            // Comandos
            ConfirmarOrdenCommand = new Command(async () => await ConfirmarOrden(), () => !IsLoading);
            EditarOrdenCommand = new Command(async () => await EditarOrden());

            // Cargar datos para el resumen
            CargarDatosResumen();
        }

        #region Propiedades

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)ConfirmarOrdenCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string NombreCliente
        {
            get => _nombreCliente;
            set { _nombreCliente = value; OnPropertyChanged(); }
        }

        public string DireccionCliente
        {
            get => _direccionCliente;
            set { _direccionCliente = value; OnPropertyChanged(); }

        }
        public string RfcCliente
        {
            get => _rfcCliente;
            set { _rfcCliente = value; OnPropertyChanged(); }
        }

        public string TelefonoCliente
        {
            get => _telefonoCliente;
            set { _telefonoCliente = value; OnPropertyChanged(); }
        }

        public string VehiculoCompleto
        {
            get => _vehiculoCompleto;
            set { _vehiculoCompleto = value; OnPropertyChanged(); }
        }

        public string VinVehiculo
        {
            get => _vinVehiculo;
            set { _vinVehiculo = value; OnPropertyChanged(); }
        }

        public string PlacasVehiculo
        {
            get => _placasVehiculo;
            set { _placasVehiculo = value; OnPropertyChanged(); }
        }

        public string TipoOrdenNombre
        {
            get => _tipoOrdenNombre;
            set { _tipoOrdenNombre = value; OnPropertyChanged(); }
        }

        public string TipoServicioNombre
        {
            get => _tipoServicioNombre;
            set { _tipoServicioNombre = value; OnPropertyChanged(); }
        }

        public decimal CostoTotal
        {
            get => _costoTotal;
            set { _costoTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(CostoTotalFormateado)); }
        }

        public string CostoTotalFormateado => $"${CostoTotal:N2}";

        public string FechaPromesaFormateada => FechaHoraPromesaEntrega.ToString("dd/MMM/yyyy");
        public string HoraPromesaFormateada => FechaHoraPromesaEntrega.ToString("hh:mm tt");
        public string KilometrajeFormateado => $"{KilometrajeActual:N0} km";
        public int CantidadTrabajos => Trabajos?.Count ?? 0;
        public bool TieneObservaciones => !string.IsNullOrWhiteSpace(ObservacionesAsesor);

        #endregion

        #region Comandos

        public ICommand ConfirmarOrdenCommand { get; }
        public ICommand EditarOrdenCommand { get; }

        #endregion

        #region Métodos

        private async void CargarDatosResumen()
        {
            IsLoading = true;

            try
            {
                // Cargar datos del cliente
                var clienteResponse = await _apiService.ObtenerClientePorIdAsync(ClienteId);
                if (clienteResponse.Success && clienteResponse.Cliente != null)
                {
                    NombreCliente = clienteResponse.Cliente.NombreCompleto;
                    DireccionCliente = $"{clienteResponse.Cliente.Colonia}, Mpio.{clienteResponse.Cliente.Municipio}, Edo.{clienteResponse.Cliente.Estado}";
                    RfcCliente = clienteResponse.Cliente.RFC;
                    TelefonoCliente = clienteResponse.Cliente.TelefonoMovil;
                }

                // Cargar datos del vehículo
                var vehiculoResponse = await _apiService.ObtenerVehiculoPorIdAsync(VehiculoId);
                if (vehiculoResponse.Success && vehiculoResponse.Vehiculo != null)
                {
                    VehiculoCompleto = vehiculoResponse.Vehiculo.VehiculoCompleto;
                    VinVehiculo = vehiculoResponse.Vehiculo.VIN;
                    PlacasVehiculo = vehiculoResponse.Vehiculo.Placas;
                }

                // Determinar tipo de orden
                TipoOrdenNombre = TipoOrdenId switch
                {
                    1 => "SERVICIO",
                    2 => "DIAGNÓSTICO",
                    3 => "REPARACIÓN",
                    4 => "GARANTÍA",
                    _ => "ORDEN"
                };

                // Cargar tipo de servicio (si aplica)
                if (TipoServicioId > 0 && TipoOrdenId == 1)
                {
                    var tiposServicio = await _apiService.ObtenerTiposServicioAsync();
                    var tipoServicio = tiposServicio.FirstOrDefault(t => t.Id == TipoServicioId);
                    if (tipoServicio != null)
                    {
                        TipoServicioNombre = tipoServicio.Nombre;
                        CostoTotal = tipoServicio.Precio;
                    }
                }

                // Calcular costo total para servicios
                if (TipoOrdenId == 1)
                {
                    var serviciosExtra = await _apiService.ObtenerServiciosFrecuentesAsync();
                    foreach (var trabajo in Trabajos.Where(t => t.Trabajo != TipoServicioNombre))
                    {
                        var servicio = serviciosExtra.FirstOrDefault(s => s.Nombre == trabajo.Trabajo);
                        if (servicio != null)
                        {
                            CostoTotal += servicio.Precio;
                        }
                    }
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

        private async Task ConfirmarOrden()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new CrearOrdenConTrabajosRequest
                {
                    TipoOrdenId = TipoOrdenId,
                    ClienteId = ClienteId,
                    VehiculoId = VehiculoId,
                    TipoServicioId = TipoServicioId,
                    KilometrajeActual = KilometrajeActual,
                    FechaHoraPromesaEntrega = FechaHoraPromesaEntrega,
                    ObservacionesAsesor = ObservacionesAsesor,
                    Trabajos = Trabajos
                };

                int asesorId = Preferences.Get("user_id", 0);
                var response = await _apiService.CrearOrdenConTrabajosAsync(request, asesorId);
                if (response.Success)
                {
                    // Mensaje de éxito
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ ¡Éxito!",
                        $"Orden {response.NumeroOrden} creada exitosamente",
                        "OK");

                    // Preguntar si desea tomar evidencias
                    bool tomarEvidencias = await Application.Current.MainPage.DisplayAlert(
                        "📸 Evidencias de recepción",
                        "¿Deseas tomar evidencias de la unidad ahora?",
                        "Sí",
                        "No");

                    if (tomarEvidencias)
                    {
                        // 🔹 Limpiar stack y luego navegar a evidencias
                        await Application.Current.MainPage.Navigation.PopToRootAsync();

                        var evidenciasPage = new EvidenciasOrdenPage(response.OrdenId);
                        await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);
                    }
                    else
                    {
                        // 🔹 Solo regresar al inicio
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                    }

                    // Notificar a otras páginas
                    MessagingCenter.Send(this, "OrdenCreada");
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al crear orden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditarOrden()
        {
            // Simplemente regresar a la página anterior (CrearOrden)
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}