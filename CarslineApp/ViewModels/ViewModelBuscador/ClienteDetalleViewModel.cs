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

            // Comandos existentes
            EditarClienteCommand = new Command(HabilitarEdicion);
            GuardarCambiosCommand = new Command(async () => await GuardarCambios());
            CancelarEdicionCommand = new Command(CancelarEdicion);
            VerVehiculoCommand = new Command<int>(async (vehiculoId) => await VerVehiculo(vehiculoId));

            // Nuevos comandos para accesos directos
            AbrirWhatsAppCommand = new Command<string>(async (telefono) => await AbrirWhatsApp(telefono));
            LlamarTelefonoCommand = new Command<string>(async (telefono) => await LlamarTelefono(telefono));
            EnviarEmailCommand = new Command<string>(async (email) => await EnviarEmail(email));
            AbrirMapaCommand = new Command(async () => await AbrirMapa());

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

        // Nuevos comandos para accesos directos
        public ICommand AbrirWhatsAppCommand { get; }
        public ICommand LlamarTelefonoCommand { get; }
        public ICommand EnviarEmailCommand { get; }
        public ICommand AbrirMapaCommand { get; }

        #endregion

        #region Métodos de Accesos Directos

        /// <summary>
        /// Abre WhatsApp con el número de teléfono especificado
        /// </summary>
        private async Task AbrirWhatsApp(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay número de teléfono disponible",
                    "OK");
                return;
            }

            try
            {
                // Limpiar el número de teléfono (quitar espacios, guiones, paréntesis)
                var telefonoLimpio = LimpiarNumeroTelefono(telefono);

                // Formato para WhatsApp (agregar código de país si no lo tiene)
                // Asume México (+52) si no tiene código de país
                if (!telefonoLimpio.StartsWith("+"))
                {
                    telefonoLimpio = "+52" + telefonoLimpio;
                }

                // URL de WhatsApp
                var whatsappUrl = $"https://wa.me/{telefonoLimpio.Replace("+", "")}";

                // Abrir WhatsApp
                await Launcher.OpenAsync(new Uri(whatsappUrl));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo abrir WhatsApp: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Realiza una llamada telefónica
        /// </summary>
        private async Task LlamarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay número de teléfono disponible",
                    "OK");
                return;
            }

            try
            {
                // Limpiar el número
                var telefonoLimpio = LimpiarNumeroTelefono(telefono);

                // Verificar si el dispositivo soporta llamadas telefónicas
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
        /// Abre el cliente de correo con el email especificado
        /// </summary>
        private async Task EnviarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "No hay correo electrónico disponible",
                    "OK");
                return;
            }

            try
            {
                // Verificar si el dispositivo soporta correo
                if (Email.Default.IsComposeSupported)
                {
                    var message = new EmailMessage
                    {
                        Subject = "",
                        Body = "",
                        To = new List<string> { email }
                    };

                    await Email.Default.ComposeAsync(message);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ No disponible",
                        "Este dispositivo no soporta correo electrónico",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo abrir el correo: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Abre Google Maps con la dirección del cliente
        /// </summary>
        private async Task AbrirMapa()
        {
            try
            {
                // Construir la dirección completa
                var direccionCompleta = ConstruirDireccionCompleta();

                if (string.IsNullOrWhiteSpace(direccionCompleta))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Advertencia",
                        "No hay dirección completa disponible",
                        "OK");
                    return;
                }

                // Codificar la dirección para URL
                var direccionCodificada = Uri.EscapeDataString(direccionCompleta);

                // URL de Google Maps
                var mapsUrl = $"https://www.google.com/maps/search/?api=1&query={direccionCodificada}";

                // Abrir en el navegador
                await Launcher.OpenAsync(new Uri(mapsUrl));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"No se pudo abrir el mapa: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Construye la dirección completa del cliente
        /// </summary>
        private string ConstruirDireccionCompleta()
        {
            var partesDireccion = new List<string>();

            if (!string.IsNullOrWhiteSpace(Calle))
                partesDireccion.Add(Calle);

            if (!string.IsNullOrWhiteSpace(NumeroExterior))
                partesDireccion.Add(NumeroExterior);

            if (!string.IsNullOrWhiteSpace(Colonia))
                partesDireccion.Add(Colonia);

            if (!string.IsNullOrWhiteSpace(Municipio))
                partesDireccion.Add(Municipio);

            if (!string.IsNullOrWhiteSpace(Estado))
                partesDireccion.Add(Estado);

            if (!string.IsNullOrWhiteSpace(CodigoPostal))
                partesDireccion.Add(CodigoPostal);

            return string.Join(", ", partesDireccion);
        }

        /// <summary>
        /// Limpia un número de teléfono quitando caracteres no numéricos
        /// </summary>
        private string LimpiarNumeroTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            // Mantener el + si está al inicio, quitar todo lo demás excepto números
            var limpio = new string(telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Asegurar que el + solo esté al inicio
            if (limpio.Contains('+'))
            {
                limpio = "+" + limpio.Replace("+", "");
            }

            return limpio;
        }

        #endregion

        #region Métodos Existentes

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
