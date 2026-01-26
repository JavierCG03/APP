using System.Collections.ObjectModel;
using CarslineApp.Models;


namespace CarslineApp.ViewModels
{
    /// <summary>
    /// Partial class para gestión de datos del vehículo
    /// </summary>
    public partial class CrearOrdenViewModel
    {
        #region Campos Privados Vehículo

        private ObservableCollection<VehiculoDto> _vehiculosEncontrados = new();
        private bool _mostrarListaVehiculos;
        private int _vehiculoId;
        private string _ultimos4VIN = string.Empty;
        private string _vin = string.Empty;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private string _version = string.Empty;
        private int _anio = DateTime.Now.Year;
        private string _color = string.Empty;
        private string _placas = string.Empty;
        private int _kilometrajeInicial;
        private bool _modoEdicionVehiculo;

        #endregion

        #region Propiedades Vehículo

        /// <summary>
        /// Lista de vehículos encontrados en la búsqueda
        /// </summary>
        public ObservableCollection<VehiculoDto> VehiculosEncontrados
        {
            get => _vehiculosEncontrados;
            set { _vehiculosEncontrados = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mostrar/ocultar lista de resultados de búsqueda
        /// </summary>
        public bool MostrarListaVehiculos
        {
            get => _mostrarListaVehiculos;
            set { _mostrarListaVehiculos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Campo de búsqueda por últimos 4 dígitos del VIN
        /// </summary>
        public string Ultimos4VIN
        {
            get => _ultimos4VIN;
            set { _ultimos4VIN = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                _vehiculoId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonEditarVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
                OnPropertyChanged(nameof(CamposVehiculoBloqueados));
            }
        }

        public string VIN
        {
            get => _vin;
            set { _vin = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int Anio
        {
            get => _anio;
            set { _anio = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public string Placas
        {
            get => _placas;
            set { _placas = value.ToUpper(); OnPropertyChanged(); }
        }

        public int KilometrajeInicial
        {
            get => _kilometrajeInicial;
            set { _kilometrajeInicial = value; OnPropertyChanged(); }
        }

        public bool ModoEdicionVehiculo
        {
            get => _modoEdicionVehiculo;
            set
            {
                _modoEdicionVehiculo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonVehiculo));
                OnPropertyChanged(nameof(ColorBotonVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
            }
        }

        public bool CampoPlacasBloqueado => VehiculoId > 0 && !ModoEdicionVehiculo;
        public bool CamposVehiculoBloqueados => VehiculoId > 0;
        public string TextoBotonVehiculo => ModoEdicionVehiculo ? "💾 Guardar Placas" : "✏️ Editar Placas";
        public string ColorBotonVehiculo => ModoEdicionVehiculo ? "#4CAF50" : "#FF9800";

        #endregion

        #region Métodos de Búsqueda de Vehículo

        /// <summary>
        /// Buscar vehículos por últimos 4 dígitos del VIN
        /// </summary>
        
        private async Task BuscarVehiculoCliente(int ClinteId)
        {
            try
            {
                var response = await _apiService.BuscarVehiculosPorClienteIdAsync(ClienteId);

                if (response.Success && response.Vehiculos != null && response.Vehiculos.Any())
                {
                    VehiculosEncontrados.Clear();
                    foreach (var vehiculo in response.Vehiculos)
                    {
                        VehiculosEncontrados.Add(vehiculo);
                    }

                    // mostrar vehículos encontrados
                    MostrarListaVehiculos = true;
                    ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";

                }
                else
                {
                    ErrorMessage = response.Message ?? "No hay vehiculos asignados a este cliente puedes registrar uno nuevo.";
                    MostrarListaVehiculos = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MostrarListaVehiculos = false;
            }
            finally
            {
                IsLoading = false;
            }

        }
        private async Task BuscarVehiculo()
        {
            if (ModoEdicionVehiculo)
            {
                await GuardarCambiosVehiculo();
                return;
            }

            if (string.IsNullOrWhiteSpace(Ultimos4VIN) || Ultimos4VIN.Length != 4)
            {
                ErrorMessage = "Ingresa exactamente 4 caracteres del VIN";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarListaVehiculos = false;

            try
            {
                var response = await _apiService.BuscarVehiculosPorUltimos4VINAsync(Ultimos4VIN);

                if (response.Success && response.Vehiculos != null && response.Vehiculos.Any())
                {
                    VehiculosEncontrados.Clear();
                    foreach (var vehiculo in response.Vehiculos)
                    {
                        VehiculosEncontrados.Add(vehiculo);
                    }

                    if (VehiculosEncontrados.Count == 1)
                    {
                        // Si solo hay un resultado, cargarlo automáticamente
                        await SeleccionarVehiculo(VehiculosEncontrados[0]);
                    }
                    else
                    {
                        // Si hay múltiples resultados, mostrar la lista
                        MostrarListaVehiculos = true;
                        ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";
                    }
                }
                else
                {
                    ErrorMessage = response.Message ?? "Vehículo no encontrado. Puedes registrar uno nuevo.";
                    MostrarListaVehiculos = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MostrarListaVehiculos = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Seleccionar vehículo de la lista de resultados
        /// </summary>
        private async Task SeleccionarVehiculo(VehiculoDto vehiculoSeleccionado)
        {
            if (vehiculoSeleccionado == null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Obtener datos completos del vehículo
                var response = await _apiService.ObtenerVehiculoPorIdAsync(vehiculoSeleccionado.Id);

                if (response.Success && response.Vehiculo != null)
                {
                    VehiculoId = response.Vehiculo.Id;
                    VIN = response.Vehiculo.VIN;
                    Marca = response.Vehiculo.Marca;
                    Modelo = response.Vehiculo.Modelo;
                    Version = response.Vehiculo.Version;
                    Anio = response.Vehiculo.Anio;
                    Color = response.Vehiculo.Color;
                    Placas = response.Vehiculo.Placas;
                    KilometrajeInicial = response.Vehiculo.KilometrajeInicial;

                    MostrarListaVehiculos = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Vehículo Seleccionado",
                        $"Se ha cargado: {response.Vehiculo.VehiculoCompleto}\nCliente: {response.Vehiculo.NombreCliente}",
                        "OK");
                }
                else
                {
                    ErrorMessage = response.Message;
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

        #endregion

        #region Métodos de Edición de Vehículo

        private async Task EditarGuardarVehiculo()
        {
            if (!ModoEdicionVehiculo)
            {
                ModoEdicionVehiculo = true;
                return;
            }

            await GuardarCambiosVehiculo();
        }

        private async Task GuardarCambiosVehiculo()
        {
            if (string.IsNullOrWhiteSpace(Placas))
            {
                ErrorMessage = "Las placas son requeridas";
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "Debes ingresar las placas del vehículo",
                    "OK");
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.ActualizarPlacasVehiculoAsync(VehiculoId, Placas);

                if (response.Success)
                {
                    ModoEdicionVehiculo = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Las placas han sido actualizadas correctamente",
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
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al actualizar placas: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Validación de Vehículo

        private bool ValidarVehiculo()
        {
            if (string.IsNullOrWhiteSpace(VIN) || VIN.Length != 17)
            {
                ErrorMessage = "El VIN debe tener 17 caracteres";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Marca))
            {
                ErrorMessage = "La marca es requerida";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Modelo))
            {
                ErrorMessage = "El modelo es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Version))
            {
                ErrorMessage = "La Version es requerida";
                return false;
            }

            if (Anio < 2000 || Anio > DateTime.Now.Year + 1)
            {
                ErrorMessage = "El año ingresado del vehiculo no es válido";
                return false;
            }

            if (KilometrajeInicial <= 0)
            {
                ErrorMessage = "Ingresa el kilometraje inicial";
                return false;
            }

            if (KilometrajeActual < KilometrajeInicial)
            {
                ErrorMessage = "El kilometraje Actual no puede ser menor al Inicial";
                return false;
            }

            return true;
        }

        #endregion
    }
}