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

            // Comandos
            EditarPlacasCommand = new Command(HabilitarEdicionPlacas);
            GuardarPlacasCommand = new Command(async () => await GuardarPlacas());
            CancelarEdicionCommand = new Command(CancelarEdicion);
            VerClienteCommand = new Command(async () => await VerCliente());
            VerOrdenCommand = new Command<int>(async (ordenId) => await VerOrden(ordenId));

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

        #endregion

        #region Comandos

        public ICommand EditarPlacasCommand { get; }
        public ICommand GuardarPlacasCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand VerClienteCommand { get; }
        public ICommand VerOrdenCommand { get; }

        #endregion

        #region Métodos

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

                    // Cargar historial de órdenes (puedes crear un endpoint específico)
                    await CargarHistorialOrdenes();
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

                    // Convertir historial a órdenes simples
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
                    Vehiculo.Placas = NuevasPlacas;
                    ModoEdicionPlacas = false;

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
            // Navegar a página de detalle de orden
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