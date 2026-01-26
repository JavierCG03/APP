using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.ViewModelBuscador
{
    public class ClienteDetalleViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _clienteId;

        private ClienteDto _cliente;
        private ObservableCollection<VehiculoDto> _vehiculosCliente;
        private bool _isLoading;
        private bool _modoEdicion;
        private string _errorMessage;

        // Campos editables (excepto nombre)
        private string _rfc;
        private string _telefonoMovil;
        private string _telefonoCasa;
        private string _correoElectronico;
        private string _colonia;
        private string _calle;
        private string _numeroExterior;
        private string _municipio;
        private string _estado;
        private string _codigoPostal;

        public ClienteDetalleViewModel(int clienteId)
        {
            _apiService = new ApiService();
            _clienteId = clienteId;
            VehiculosCliente = new ObservableCollection<VehiculoDto>();

            // Comandos
            EditarClienteCommand = new Command(HabilitarEdicion);
            GuardarCambiosCommand = new Command(async () => await GuardarCambios());
            CancelarEdicionCommand = new Command(CancelarEdicion);
            VerVehiculoCommand = new Command<int>(async (vehiculoId) => await VerVehiculo(vehiculoId));

            CargarDatosCliente();
        }

        #region Propiedades

        public ClienteDto Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        public ObservableCollection<VehiculoDto> VehiculosCliente
        {
            get => _vehiculosCliente;
            set { _vehiculosCliente = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool ModoEdicion
        {
            get => _modoEdicion;
            set
            {
                _modoEdicion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CamposBloqueados));
                OnPropertyChanged(nameof(MostrarBotonesEdicion));
                OnPropertyChanged(nameof(MostrarBotonEditar));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool CamposBloqueados => !ModoEdicion;
        public bool MostrarBotonesEdicion => ModoEdicion;
        public bool MostrarBotonEditar => !ModoEdicion;
        public bool TieneVehiculos => VehiculosCliente?.Any() ?? false;

        // Propiedades editables
        public string RFC
        {
            get => _rfc;
            set { _rfc = value?.ToUpper(); OnPropertyChanged(); }
        }

        public string TelefonoMovil
        {
            get => _telefonoMovil;
            set { _telefonoMovil = value; OnPropertyChanged(); }
        }

        public string TelefonoCasa
        {
            get => _telefonoCasa;
            set { _telefonoCasa = value; OnPropertyChanged(); }
        }

        public string CorreoElectronico
        {
            get => _correoElectronico;
            set { _correoElectronico = value; OnPropertyChanged(); }
        }

        public string Colonia
        {
            get => _colonia;
            set { _colonia = value; OnPropertyChanged(); }
        }

        public string Calle
        {
            get => _calle;
            set { _calle = value; OnPropertyChanged(); }
        }

        public string NumeroExterior
        {
            get => _numeroExterior;
            set { _numeroExterior = value; OnPropertyChanged(); }
        }

        public string Municipio
        {
            get => _municipio;
            set { _municipio = value; OnPropertyChanged(); }
        }

        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        public string CodigoPostal
        {
            get => _codigoPostal;
            set { _codigoPostal = value; OnPropertyChanged(); }
        }

        #endregion

        #region Comandos

        public ICommand EditarClienteCommand { get; }
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand VerVehiculoCommand { get; }

        #endregion

        #region Métodos

        private async void CargarDatosCliente()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Cargar datos del cliente
                var clienteResponse = await _apiService.ObtenerClientePorIdAsync(_clienteId);
                if (clienteResponse.Success && clienteResponse.Cliente != null)
                {
                    Cliente = clienteResponse.Cliente;

                    // Cargar datos en propiedades editables
                    RFC = Cliente.RFC;
                    TelefonoMovil = Cliente.TelefonoMovil;
                    TelefonoCasa = Cliente.TelefonoCasa;
                    CorreoElectronico = Cliente.CorreoElectronico;
                    Colonia = Cliente.Colonia;
                    Calle = Cliente.Calle;
                    NumeroExterior = Cliente.NumeroExterior;
                    Municipio = Cliente.Municipio;
                    Estado = Cliente.Estado;
                    CodigoPostal = Cliente.CodigoPostal;

                    // Cargar vehículos del cliente
                    await CargarVehiculosCliente();
                }
                else
                {
                    ErrorMessage = clienteResponse.Message;
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

        private async Task CargarVehiculosCliente()
        {
            try
            {
                var vehiculosResponse = await _apiService.BuscarVehiculosPorClienteIdAsync(_clienteId);

                if (vehiculosResponse.Success && vehiculosResponse.Vehiculos != null)
                {
                    VehiculosCliente.Clear();
                    foreach (var vehiculo in vehiculosResponse.Vehiculos)
                    {
                        VehiculosCliente.Add(vehiculo);
                    }
                    OnPropertyChanged(nameof(TieneVehiculos));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar vehículos: {ex.Message}");
            }
        }

        private void HabilitarEdicion()
        {
            ModoEdicion = true;
        }

        private async Task GuardarCambios()
        {
            if (!ValidarDatos()) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new ClienteRequest
                {
                    NombreCompleto = Cliente.NombreCompleto, // No editable
                    RFC = RFC,
                    TelefonoMovil = TelefonoMovil,
                    TelefonoCasa = TelefonoCasa,
                    CorreoElectronico = CorreoElectronico,
                    Colonia = Colonia,
                    Calle = Calle,
                    NumeroExterior = NumeroExterior,
                    Municipio = Municipio,
                    Estado = Estado,
                    CodigoPostal = CodigoPostal
                };

                var response = await _apiService.ActualizarClienteAsync(_clienteId, request);

                if (response.Success)
                {
                    ModoEdicion = false;

                    // Actualizar datos del cliente
                    Cliente.RFC = RFC;
                    Cliente.TelefonoMovil = TelefonoMovil;
                    Cliente.TelefonoCasa = TelefonoCasa;
                    Cliente.CorreoElectronico = CorreoElectronico;
                    Cliente.Colonia = Colonia;
                    Cliente.Calle = Calle;
                    Cliente.NumeroExterior = NumeroExterior;
                    Cliente.Municipio = Municipio;
                    Cliente.Estado = Estado;
                    Cliente.CodigoPostal = CodigoPostal;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Datos del cliente actualizados correctamente",
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
            // Restaurar valores originales
            RFC = Cliente.RFC;
            TelefonoMovil = Cliente.TelefonoMovil;
            TelefonoCasa = Cliente.TelefonoCasa;
            CorreoElectronico = Cliente.CorreoElectronico;
            Colonia = Cliente.Colonia;
            Calle = Cliente.Calle;
            NumeroExterior = Cliente.NumeroExterior;
            Municipio = Cliente.Municipio;
            Estado = Cliente.Estado;
            CodigoPostal = Cliente.CodigoPostal;

            ModoEdicion = false;
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(RFC) || RFC.Length < 12)
            {
                Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "El RFC es requerido (mínimo 12 caracteres)",
                    "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelefonoMovil))
            {
                Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "El teléfono móvil es requerido",
                    "OK");
                return false;
            }

            return true;
        }

        private async Task VerVehiculo(int vehiculoId)
        {
            var vehiculoPage = new Views.Buscador.VehiculosPage(vehiculoId);
            await Application.Current.MainPage.Navigation.PushAsync(vehiculoPage);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}