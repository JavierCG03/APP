
using CarslineApp.Models;
using CarslineApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// ViewModel principal para crear órdenes de servicio
    /// </summary>
    public partial class CrearOrdenViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _tipoOrdenId;

        private int _pasoActual = 1; // 1=Cliente, 2=Vehículo, 3=Orden
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        
        public CrearOrdenViewModel(int tipoOrdenId)
        {
            _tipoOrdenId = tipoOrdenId;
            _apiService = new ApiService();

            // Comandos
            BuscarClienteCommand = new Command(async () => await BuscarCliente());
            SeleccionarClienteCommand = new Command<ClienteDto>(async (cliente) => await SeleccionarCliente(cliente));
            BuscarVehiculoCommand = new Command(async () => await BuscarVehiculo());
            SeleccionarVehiculoCommand = new Command<VehiculoDto>(async (vehiculo) => await SeleccionarVehiculo(vehiculo));
            HabilitarEdicionClienteCommand = new Command(() => ModoEdicionCliente = true);
            HabilitarEdicionVehiculoCommand = new Command(() => ModoEdicionVehiculo = true);
            SiguienteCommand = new Command(async () => await Siguiente(), () => !IsLoading);
            AnteriorCommand = new Command(() => Anterior());
            CrearOrdenServicioCommand = new Command(async () => await CrearOrdenServicio(), () => !IsLoading);
            CrearOrdenReparacionCommand = new Command(async () => await CrearOrdenReparacion(), () => !IsLoading);
            CrearOrdenDiagnosticoCommand = new Command(async () => await CrearOrdenDiagnostico(), () => !IsLoading);
            CrearOrdenGarantiaCommand = new Command(async () => await CrearOrdenGarantia(), () => !IsLoading);
            EditarGuardarClienteCommand = new Command(async () => await EditarGuardarCliente());
            EditarGuardarVehiculoCommand = new Command(async () => await EditarGuardarVehiculo());
            AgregarTrabajoPersonalizadoCommand = new Command(AgregarTrabajoPersonalizado);
            EliminarTrabajoPersonalizadoCommand = new Command<TrabajoCrearDto>(EliminarTrabajoPersonalizado);

            CargarCatalogos();

            OnPropertyChanged(nameof(EsServicio));
            OnPropertyChanged(nameof(EsDiagnostico));
            OnPropertyChanged(nameof(EsReparacion));
            OnPropertyChanged(nameof(EsGarantia));
        }

        #region Propiedades Generales

        public int PasoActual
        {
            get => _pasoActual;
            set
            {
                _pasoActual = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarPaso1));
                OnPropertyChanged(nameof(MostrarPaso2));
                OnPropertyChanged(nameof(MostrarPaso3));
                OnPropertyChanged(nameof(MostrarBotonSiguiente));
                OnPropertyChanged(nameof(MostrarBotonCrear));
                OnPropertyChanged(nameof(TituloPaso));
            }
        }

        public bool MostrarBotonEditarCliente => ClienteId > 0;
        public bool MostrarBotonEditarVehiculo => VehiculoId > 0;
        public bool MostrarPaso1 => PasoActual == 1;
        public bool MostrarPaso2 => PasoActual == 2;
        public bool MostrarPaso3 => PasoActual == 3;
        public bool MostrarBotonSiguiente => PasoActual < 3;
        public bool MostrarBotonCrear => PasoActual == 3;

        public bool EsServicio => _tipoOrdenId == 1;
        public bool EsDiagnostico => _tipoOrdenId == 2;
        public bool EsReparacion => _tipoOrdenId == 3;
        public bool EsGarantia => _tipoOrdenId == 4;

        public string TituloPaso => PasoActual switch
        {
            1 => "DATOS DEL CLIENTE",
            2 => "DATOS DEL VEHÍCULO",
            3 => _tipoOrdenId switch
            {
                1 => "DATOS DEL SERVICIO",
                2 => "DATOS DEL DIAGNÓSTICO",
                3 => "DATOS DE LA REPARACIÓN",
                4 => "DATOS DE LA GARANTÍA",
                _ => ""
            },
            _ => "CREAR ORDEN"
        };

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)SiguienteCommand).ChangeCanExecute();
                ((Command)CrearOrdenServicioCommand).ChangeCanExecute();
                ((Command)CrearOrdenReparacionCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Comandos

        public ICommand HabilitarEdicionClienteCommand { get; }
        public ICommand HabilitarEdicionVehiculoCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand BuscarVehiculoCommand { get; }
        public ICommand SeleccionarVehiculoCommand { get; }
        public ICommand SiguienteCommand { get; }
        public ICommand AnteriorCommand { get; }
        public ICommand CrearOrdenCommand { get; }
        public ICommand CrearOrdenServicioCommand { get; }
        public ICommand CrearOrdenReparacionCommand { get; }
        public ICommand CrearOrdenDiagnosticoCommand { get; }
        public ICommand CrearOrdenGarantiaCommand { get; }
        public ICommand EditarGuardarClienteCommand { get; }
        public ICommand EditarGuardarVehiculoCommand { get; }
        public ICommand AgregarTrabajoPersonalizadoCommand { get; }
        public ICommand EliminarTrabajoPersonalizadoCommand { get; }


        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Métodos de Navegación

        private void Anterior()
        {
            if (PasoActual > 1)
            {
                PasoActual--;

                // Ocultar listas al regresar
                if (PasoActual == 1)
                {
                    MostrarListaClientes = false;
                }
                else if (PasoActual == 2)
                {
                    MostrarListaVehiculos = false;
                }
            }
        }

        private async Task Siguiente()
        {
            ErrorMessage = string.Empty;

            if (PasoActual == 1)
            {
                if (ModoEdicionCliente)
                {
                    ErrorMessage = "Debes guardar los cambios del cliente antes de continuar";
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Por favor, guarda los cambios del cliente antes de continuar",
                        "OK");
                    return;
                }

                if (!ValidarCliente()) return;

                if (ClienteId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        var request = new ClienteRequest
                        {
                            NombreCompleto = NombreCompleto,
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

                        var response = await _apiService.CrearClienteAsync(request);

                        if (response.Success)
                        {
                            ClienteId = response.ClienteId;
                        }
                        else
                        {
                            ErrorMessage = response.Message;
                            return;
                        }
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
                BuscarVehiculoCliente(ClienteId);
                PasoActual = 2;
            }
            else if (PasoActual == 2)
            {
                if (ModoEdicionVehiculo)
                {
                    ErrorMessage = "Debes guardar los cambios de las placas antes de continuar";
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Por favor, guarda los cambios de las placas antes de continuar",
                        "OK");
                    return;
                }

                if (!ValidarVehiculo()) return;

                if (VehiculoId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        var request = new VehiculoRequest
                        {
                            ClienteId = ClienteId,
                            VIN = VIN,
                            Marca = Marca,
                            Modelo = Modelo,
                            Version = Version,
                            Anio = Anio,
                            Color = Color,
                            Placas = Placas,
                            KilometrajeInicial = KilometrajeInicial
                        };

                        var response = await _apiService.CrearVehiculoAsync(request);

                        if (response.Success)
                        {
                            VehiculoId = response.VehiculoId;
                        }
                        else
                        {
                            ErrorMessage = response.Message;
                            return;
                        }
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }

                // Cargar historial al pasar al paso 3
                await CargarHistorialVehiculo();
                // Recalcular servicio subsecuente cuando cambie el kilometraje y tip de orden sea Servicio
                if (TieneHistorial && KilometrajeActual > 0 && _tipoOrdenId == 1)
                {
                    CalcularServicioSubsecuente();
                }
                else if (TieneHistorial && KilometrajeActual > 0 && _tipoOrdenId == 4)
                {
                    ValidacionGarantia();
                }

                PasoActual = 3;
            }
        }

        #endregion

        #region Métodos Auxiliares

        private async void CargarCatalogos()
        {
            try
            {
                var tipos = await _apiService.ObtenerTiposServicioAsync();
                TiposServicio.Clear();
                foreach (var tipo in tipos)
                {
                    TiposServicio.Add(tipo);
                }

                var extras = await _apiService.ObtenerServiciosFrecuentesAsync();
                ServiciosExtra.Clear();
                foreach (var extra in extras)
                {
                    ServiciosExtra.Add(extra);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar catálogos: {ex.Message}";
            }
        }

        #endregion
    }
}