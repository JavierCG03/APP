using System.Collections.ObjectModel;
using CarslineApp.Models;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// Partial class para gestión de datos del cliente
    /// </summary>
    public partial class CrearOrdenViewModel
    {
        #region Campos Privados Cliente

        private ObservableCollection<ClienteDto> _clientesEncontrados = new();
        private bool _mostrarListaClientes;
        private int _clienteId;
        private string _nombreBusquedaCliente = string.Empty;
        private string _rfc = string.Empty;
        private string _nombreCompleto = string.Empty;
        private string _telefonoMovil = string.Empty;
        private string _telefonoCasa = string.Empty;
        private string _correoElectronico = string.Empty;
        private string _colonia = string.Empty;
        private string _calle = string.Empty;
        private string _numeroExterior = string.Empty;
        private string _municipio = string.Empty;
        private string _estado = string.Empty;
        private string _codigoPostal = string.Empty;
        private bool _modoEdicionCliente;

        #endregion

        #region Propiedades Cliente

        /// <summary>
        /// Lista de clientes encontrados en la búsqueda
        /// </summary>
        public ObservableCollection<ClienteDto> ClientesEncontrados
        {
            get => _clientesEncontrados;
            set { _clientesEncontrados = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mostrar/ocultar lista de resultados de búsqueda
        /// </summary>
        public bool MostrarListaClientes
        {
            get => _mostrarListaClientes;
            set { _mostrarListaClientes = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Campo de búsqueda por nombre del cliente
        /// </summary>
        public string NombreBusquedaCliente
        {
            get => _nombreBusquedaCliente;
            set { _nombreBusquedaCliente = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int ClienteId
        {
            get => _clienteId;
            set
            {
                _clienteId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonEditarCliente));
                OnPropertyChanged(nameof(CamposClienteBloqueados));
            }
        }

        public string RFC
        {
            get => _rfc;
            set { _rfc = value.ToUpper(); OnPropertyChanged(); }
        }

        public string NombreCompleto
        {
            get => _nombreCompleto;
            set { _nombreCompleto = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string TelefonoMovil
        {
            get => _telefonoMovil;
            set { _telefonoMovil = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
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

        public bool ModoEdicionCliente
        {
            get => _modoEdicionCliente;
            set
            {
                _modoEdicionCliente = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonCliente));
                OnPropertyChanged(nameof(ColorBotonCliente));
                OnPropertyChanged(nameof(CamposClienteBloqueados));
            }
        }

        public bool CamposClienteBloqueados => ClienteId > 0 && !ModoEdicionCliente;
        public string TextoBotonCliente => ModoEdicionCliente ? "💾 Guardar Cambios" : "✏️ Editar";
        public string ColorBotonCliente => ModoEdicionCliente ? "#4CAF50" : "#FF9800";

        #endregion

        #region Métodos de Búsqueda de Cliente

        /// <summary>
        /// Buscar clientes por nombre
        /// </summary>
        private async Task BuscarCliente()
        {
            if (ModoEdicionCliente)
            {
                await GuardarCambiosCliente();
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreBusquedaCliente) || NombreBusquedaCliente.Length < 3)
            {
                ErrorMessage = "Ingresa al menos 3 caracteres del nombre";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarListaClientes = false;

            try
            {
                var response = await _apiService.BuscarClientesPorNombreAsync(NombreBusquedaCliente);

                if (response.Success && response.Clientes != null && response.Clientes.Any())
                {
                    ClientesEncontrados.Clear();
                    foreach (var cliente in response.Clientes)
                    {
                        ClientesEncontrados.Add(cliente);
                    }

                    if (ClientesEncontrados.Count == 1)
                    {
                        // Si solo hay un resultado, cargarlo automáticamente
                        await SeleccionarCliente(ClientesEncontrados[0]);
                    }
                    else
                    {
                        // Si hay múltiples resultados, mostrar la lista
                        MostrarListaClientes = true;
                        ErrorMessage = $"Se encontraron {ClientesEncontrados.Count} clientes. Selecciona uno:";
                    }
                }
                else
                {
                    ErrorMessage = response.Message ?? "Cliente no encontrado. Puedes registrar uno nuevo.";
                    MostrarListaClientes = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MostrarListaClientes = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Seleccionar cliente de la lista de resultados
        /// </summary>
        private async Task SeleccionarCliente(ClienteDto clienteSeleccionado)
        {
            if (clienteSeleccionado == null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Obtener datos completos del cliente
                var response = await _apiService.ObtenerClientePorIdAsync(clienteSeleccionado.Id);

                if (response.Success && response.Cliente != null)
                {
                    ClienteId = response.Cliente.Id;
                    NombreCompleto = response.Cliente.NombreCompleto;
                    RFC = response.Cliente.RFC;
                    TelefonoMovil = response.Cliente.TelefonoMovil;
                    TelefonoCasa = response.Cliente.TelefonoCasa ?? "";
                    CorreoElectronico = response.Cliente.CorreoElectronico ?? "";
                    Colonia = response.Cliente.Colonia ?? "";
                    Calle = response.Cliente.Calle ?? "";
                    NumeroExterior = response.Cliente.NumeroExterior ?? "";
                    Municipio = response.Cliente.Municipio ?? "";
                    Estado = response.Cliente.Estado ?? "";
                    CodigoPostal = response.Cliente.CodigoPostal ?? "";

                    MostrarListaClientes = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Cliente Seleccionado",
                        $"Se han cargado los datos de {response.Cliente.NombreCompleto}",
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

        #region Métodos de Edición de Cliente

        private async Task EditarGuardarCliente()
        {
            if (!ModoEdicionCliente)
            {
                ModoEdicionCliente = true;
                return;
            }
            await GuardarCambiosCliente();
        }

        private async Task GuardarCambiosCliente()
        {
            if (!ValidarCliente()) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

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

                var response = await _apiService.ActualizarClienteAsync(ClienteId, request);

                if (response.Success)
                {
                    ModoEdicionCliente = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Los datos del cliente han sido actualizados correctamente",
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
                    $"Error al actualizar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Validación de Cliente

        private bool ValidarCliente()
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto))
            {
                ErrorMessage = "El nombre completo es requerido";
                return false;
            }
            if (string.IsNullOrWhiteSpace(RFC) || RFC.Length < 12)
            {
                ErrorMessage = "El RFC es requerido (mínimo 12 caracteres)";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TelefonoMovil))
            {
                ErrorMessage = "El teléfono móvil es requerido";
                return false;
            }
            return true;
        }
        #endregion
    }
}