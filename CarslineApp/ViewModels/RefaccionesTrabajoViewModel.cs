using CarslineApp.Services;
using CarslineApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    public class RefaccionesTrabajoViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _trabajoId;

        private ObservableCollection<RefaccionTrabajoViewModel> _refacciones;
        private bool _estaCargando;
        private decimal _totalRefacciones;
        private decimal _precioManoObra;
        private decimal _manoObraOriginal;
        private string _nuevaRefaccion = string.Empty;
        private string _nuevaCantidad = string.Empty;
        private string _nuevoPrecioUnitario = string.Empty;
        private string _precioManoObraTexto = string.Empty;
        private bool _formularioExpandido = false;
        private bool _manoObraExpandido = false;

        // Nuevas propiedades para información del trabajo
        private string _nombreTrabajo = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vin = string.Empty;
        private bool _infoTrabajoVisible = true;

        public RefaccionesTrabajoViewModel(int trabajoId)
        {
            _apiService = new ApiService();
            _trabajoId = trabajoId;
            _refacciones = new ObservableCollection<RefaccionTrabajoViewModel>();

            // Comandos
            AgregarRefaccionCommand = new Command(async () => await AgregarRefaccion(), () => !EstaCargando);
            EliminarRefaccionCommand = new Command<RefaccionTrabajoViewModel>(async (refaccion) => await EliminarRefaccion(refaccion));
            EditarManoObraCommand = new Command(async () => await GuardarManoObra());
            ToggleFormularioCommand = new Command(() => FormularioExpandido = !FormularioExpandido);
            ToggleManoObraCommand = new Command(() => ManoObraExpandido = !ManoObraExpandido);
            ToggleInfoCommand = new Command(() => InfoTrabajoVisible = !InfoTrabajoVisible);
        }

        #region Propiedades

        public ObservableCollection<RefaccionTrabajoViewModel> Refacciones
        {
            get => _refacciones;
            set
            {
                _refacciones = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalRefacciones
        {
            get => _totalRefacciones;
            set
            {
                _totalRefacciones = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalRefaccionesFormateado));
                CalcularTotales();
            }
        }

        public decimal PrecioManoObra
        {
            get => _precioManoObra;
            set
            {
                _precioManoObra = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ManoObraFormateado));
                CalcularTotales();
            }
        }

        public string PrecioManoObraTexto
        {
            get => _precioManoObraTexto;
            set
            {
                _precioManoObraTexto = value;
                OnPropertyChanged();
            }
        }

        public bool FormularioExpandido
        {
            get => _formularioExpandido;
            set
            {
                _formularioExpandido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoFormulario));
            }
        }

        public bool ManoObraExpandido
        {
            get => _manoObraExpandido;
            set
            {
                _manoObraExpandido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoManoObra));
            }
        }
        public bool InfoTrabajoVisible
        {
            get => _infoTrabajoVisible;
            set
            {
                _infoTrabajoVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoInfo));
            }
        }
        public string IconoFormulario => FormularioExpandido ? "▲" : "▼";
        public string IconoManoObra => ManoObraExpandido ? "▲" : "▼";
        public string IconoInfo => InfoTrabajoVisible ? "▲" : "▼";

        // Nuevas propiedades para información del trabajo
        public string NombreTrabajo
        {
            get => _nombreTrabajo;
            set
            {
                _nombreTrabajo = value;
                OnPropertyChanged();
            }
        }

        public string VehiculoCompleto
        {
            get => _vehiculoCompleto;
            set
            {
                _vehiculoCompleto = value;
                OnPropertyChanged();
            }
        }

        public string VIN
        {
            get => _vin;
            set
            {
                _vin = value;
                OnPropertyChanged();
            }
        }


        // Propiedades calculadas
        public decimal Subtotal => TotalRefacciones + PrecioManoObra;
        public decimal Iva => Subtotal * 0.16m;
        public decimal TotalGeneral => Subtotal + Iva;

        // Propiedades formateadas
        public string TotalRefaccionesFormateado => $"${TotalRefacciones:N2}";
        public string ManoObraFormateado => $"${PrecioManoObra:N2}";
        public string SubtotalFormateado => $"${Subtotal:N2}";
        public string IvaFormateado => $"${Iva:N2}";
        public string TotalGeneralFormateado => $"${TotalGeneral:N2}";

        public bool EstaCargando
        {
            get => _estaCargando;
            set
            {
                _estaCargando = value;
                OnPropertyChanged();
                ((Command)AgregarRefaccionCommand).ChangeCanExecute();
            }
        }

        public string NuevaRefaccion
        {
            get => _nuevaRefaccion;
            set
            {
                _nuevaRefaccion = value;
                OnPropertyChanged();
            }
        }

        public string NuevaCantidad
        {
            get => _nuevaCantidad;
            set
            {
                _nuevaCantidad = value;
                OnPropertyChanged();
            }
        }

        public string NuevoPrecioUnitario
        {
            get => _nuevoPrecioUnitario;
            set
            {
                _nuevoPrecioUnitario = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand AgregarRefaccionCommand { get; }
        public ICommand EliminarRefaccionCommand { get; }
        public ICommand EditarManoObraCommand { get; }
        public ICommand ToggleFormularioCommand { get; }
        public ICommand ToggleManoObraCommand { get; }
        public ICommand ToggleInfoCommand { get; }
        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            await CargarInformacionTrabajo();
            await CargarRefacciones();
            await CargarManoObra();
        }

        #endregion

        #region Métodos Privados

        private async Task CargarInformacionTrabajo()
        {
            EstaCargando = true;

            try
            {
                var response = await _apiService.ObtenerInfoTrabajo(_trabajoId);

                if (response.Success)
                {
                    NombreTrabajo = response.Trabajo;
                    VehiculoCompleto = response.VehiculoCompleto;
                    VIN = response.VIN;
                    InfoTrabajoVisible = true;

                    System.Diagnostics.Debug.WriteLine($"✅ Información del trabajo cargada: {NombreTrabajo}");
                }
                else
                {
                    InfoTrabajoVisible = false;
                    System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo cargar información del trabajo: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar información del trabajo: {ex.Message}");
                InfoTrabajoVisible = false;
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CargarRefacciones()
        {
            EstaCargando = true;

            try
            {
                var response = await _apiService.ObtenerRefaccionesPorTrabajo(_trabajoId);

                if (response.Success)
                {
                    Refacciones.Clear();
                    foreach (var refaccion in response.Refacciones)
                    {
                        Refacciones.Add(new RefaccionTrabajoViewModel(refaccion));
                    }
                    TotalRefacciones = response.TotalRefacciones;

                    System.Diagnostics.Debug.WriteLine($"✅ Se cargaron {Refacciones.Count} refacciones. Total: {TotalRefaccionesFormateado}");
                }
                else
                {
                    await MostrarAlerta("Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar refacciones: {ex.Message}");
                await MostrarAlerta("Error", "No se pudieron cargar las refacciones");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CargarManoObra()
        {
            EstaCargando = true;

            try
            {
                var response = await _apiService.ObtenerCostoManoObraAsync(_trabajoId);

                if (response.Success)
                {
                    PrecioManoObra = response.CostoManoObra;
                    _manoObraOriginal = response.CostoManoObra;
                    PrecioManoObraTexto =  PrecioManoObra.ToString("F2");

                    System.Diagnostics.Debug.WriteLine($"✅ Mano de obra cargada: {ManoObraFormateado}");
                }
                else
                {
                    PrecioManoObra = 0;
                    _manoObraOriginal = 0;
                    PrecioManoObraTexto = "0.00";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al cargar mano de obra: {ex.Message}");
                PrecioManoObra = 0;
                _manoObraOriginal = 0;
                PrecioManoObraTexto = "0.00";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task GuardarManoObra()
        {
            EstaCargando = true;

            try
            {
                if (!decimal.TryParse(PrecioManoObraTexto, out decimal nuevoPrecio) || nuevoPrecio < 0)
                {
                    await MostrarAlerta("Campo inválido", "Ingresa un precio válido");
                    return;
                }

                if (nuevoPrecio != _manoObraOriginal)
                {
                    var response = await _apiService.FijarCostoManoObraAsync(_trabajoId, nuevoPrecio);

                    if (response.Success)
                    {
                        PrecioManoObra = nuevoPrecio;
                        _manoObraOriginal = nuevoPrecio;
                        PrecioManoObraTexto = nuevoPrecio.ToString("F2");

                        await MostrarAlerta("✅ Éxito", "Mano de obra actualizada correctamente");
                    }
                    else
                    {
                        await MostrarAlerta("Error", response.Message);
                        PrecioManoObraTexto = _manoObraOriginal.ToString("F2");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar mano de obra: {ex.Message}");
                await MostrarAlerta("Error", "No se pudo actualizar la mano de obra");
                PrecioManoObraTexto = _manoObraOriginal.ToString("F2");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task AgregarRefaccion()
        {
            if (string.IsNullOrWhiteSpace(NuevaRefaccion))
            {
                await MostrarAlerta("Campo requerido", "Ingresa el nombre de la refacción");
                return;
            }

            if (string.IsNullOrWhiteSpace(NuevaCantidad) || !int.TryParse(NuevaCantidad, out int cantidad) || cantidad <= 0)
            {
                await MostrarAlerta("Campo inválido", "Ingresa una cantidad válida");
                return;
            }

            if (string.IsNullOrWhiteSpace(NuevoPrecioUnitario) || !decimal.TryParse(NuevoPrecioUnitario, out decimal precioUnitario) || precioUnitario <= 0)
            {
                await MostrarAlerta("Campo inválido", "Ingresa un precio unitario válido");
                return;
            }

            EstaCargando = true;

            try
            {
                var nuevaRefaccion = new AgregarRefaccionDto
                {
                    Refaccion = NuevaRefaccion.Trim(),
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario
                };

                var request = new AgregarRefaccionesTrabajoRequest
                {
                    TrabajoId = _trabajoId,
                    Refacciones = new List<AgregarRefaccionDto> { nuevaRefaccion }
                };

                var response = await _apiService.AgregarRefaccionesTrabajo(request);

                if (response.Success)
                {
                    NuevaRefaccion = string.Empty;
                    NuevaCantidad = string.Empty;
                    NuevoPrecioUnitario = string.Empty;

                    await CargarRefacciones();

                    await MostrarAlerta("✅ Éxito", "Refacción agregada correctamente");
                }
                else
                {
                    await MostrarAlerta("Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al agregar refacción: {ex.Message}");
                await MostrarAlerta("Error", "No se pudo agregar la refacción");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task EliminarRefaccion(RefaccionTrabajoViewModel refaccion)
        {
            if (refaccion == null) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Eliminar la refacción:\n{refaccion.Nombre}?",
                "Sí, eliminar",
                "Cancelar");

            if (!confirmar) return;

            EstaCargando = true;

            try
            {
                var response = await _apiService.EliminarRefaccionTrabajo(refaccion.Id);

                if (response.Success)
                {
                    await CargarRefacciones();
                    await MostrarAlerta("✅ Éxito", "Refacción eliminada correctamente");
                }
                else
                {
                    await MostrarAlerta("Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar refacción: {ex.Message}");
                await MostrarAlerta("Error", "No se pudo eliminar la refacción");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void CalcularTotales()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Iva));
            OnPropertyChanged(nameof(TotalGeneral));
            OnPropertyChanged(nameof(SubtotalFormateado));
            OnPropertyChanged(nameof(IvaFormateado));
            OnPropertyChanged(nameof(TotalGeneralFormateado));
        }

        private async Task MostrarAlerta(string titulo, string mensaje)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error mostrando alerta: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}