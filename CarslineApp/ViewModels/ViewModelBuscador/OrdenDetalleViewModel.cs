using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using CarslineApp.Views.Buscador;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelBuscador
{
    public class OrdenDetalleViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _ordenId;
        private OrdenConTrabajosDto _orden;
        private ClienteDto _cliente;
        private VehiculoDto _vehiculo;
        private bool _isLoading;
        private string _errorMessage;

        public OrdenDetalleViewModel(int ordenId)
        {
            _apiService = new ApiService();
            _ordenId = ordenId;

            // Comandos
            VerClienteCommand = new Command(async () => await VerCliente());
            VerVehiculoCommand = new Command(async () => await VerVehiculo());
            VerRefaccionesCommand = new Command<int>(async (id) => await VerRefaccionesTrabajo(id));
            VerEvidenciasCommand = new Command(async () => await VerEvidencias());
            CancelarOrdenCommand = new Command(async () => await CancelarOrden());
            EntregarOrdenCommand = new Command(async () => await EntregarOrden());
            RefreshCommand = new Command(async () => await CargarDatosOrden());
            VerEvidenciasTrabajoCommand = new Command(async () => await VerEvidenciasTrabajo());
            GenerarPdfCommand = new Command(async () => await OnVerReporte());
            _ = CargarDatosOrden();
        }

        #region Propiedades

        public OrdenConTrabajosDto Orden
        {
            get => _orden;
            set
            {
                System.Diagnostics.Debug.WriteLine($"📝 SET Orden - Antes: {(_orden == null ? "NULL" : _orden.NumeroOrden)}");
                System.Diagnostics.Debug.WriteLine($"📝 SET Orden - Nuevo: {(value == null ? "NULL" : value.NumeroOrden)}");

                _orden = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneOrden));
                OnPropertyChanged(nameof(EsPendiente));
                OnPropertyChanged(nameof(EsEnProceso));
                OnPropertyChanged(nameof(EsFinalizada));
                OnPropertyChanged(nameof(PuedeEntregar));
                OnPropertyChanged(nameof(PuedeCancelar));
                OnPropertyChanged(nameof(ColorEstado));
                OnPropertyChanged(nameof(IconoEstado));

                // Actualizar comando
                try
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Actualizando CanExecute de GenerarPdfCommand...");
                    ((Command)GenerarPdfCommand).ChangeCanExecute();
                    System.Diagnostics.Debug.WriteLine($"✅ CanExecute actualizado - TieneOrden: {TieneOrden}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error actualizando CanExecute: {ex.Message}");
                }
            }
        }
        public ClienteDto Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        public VehiculoDto Vehiculo
        {
            get => _vehiculo;
            set { _vehiculo = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Propiedades calculadas
        public bool TieneOrden => Orden != null;
        public bool EsPendiente => Orden?.EstadoOrdenId == 1;
        public bool EsEnProceso => Orden?.EstadoOrdenId == 2;
        public bool EsFinalizada => Orden?.EstadoOrdenId == 3;
        public bool PuedeEntregar => EsFinalizada && Orden?.ProgresoGeneral >= 100;
        public bool PuedeCancelar => EsPendiente ;

        public string ColorEstado => Orden?.EstadoOrdenId switch
        {
            1 => "#FFA500", // Pendiente - Naranja
            2 => "#2196F3", // En Proceso - Azul
            3 => "#00BCD4", // Finalizada - Turquesa
            4 => "#4CAF50", // Entregada - Verde 
            5 => "#757575", // Cancelada - Gris oscuro
            _ => "#757575"  // Desconocido - Gris
        };

        public string IconoEstado => Orden?.EstadoOrdenId switch
        {
            1 => "📋",  // Pendiente
            2 => "⚙️",  // En Proceso
            3 => "✔️",  // Finalizada
            4 => "✅",  // Entregada
            5 =>  "❌",  // Cancelada          
            _ => "❓"   // Desconocido
        };
        #endregion

        #region Comandos

        public ICommand VerClienteCommand { get; }
        public ICommand VerVehiculoCommand { get; }
        public ICommand VerRefaccionesCommand { get; }
        public ICommand VerEvidenciasCommand { get; }
        public ICommand CancelarOrdenCommand { get; }
        public ICommand EntregarOrdenCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand VerEvidenciasTrabajoCommand { get; }
        public ICommand GenerarPdfCommand { get; }

        #endregion


        private async Task OnVerReporte()
        {
            try
            {
                IsLoading = true;

                // Validar que tengas un OrdenId válido
                if (_ordenId <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No hay una orden seleccionada para generar el reporte",
                        "OK");
                    return;
                }

                // Navegar a la página de reporte pasando el OrdenId y el ApiService
                await Application.Current.MainPage.Navigation.PushAsync(
                    new VerReportePage(_ordenId));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir el reporte: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        #region Métodos
        private async Task VerEvidenciasTrabajo()
        {
            if (Orden == null) return;

            try
            {
                var evidenciasPage = new EvidenciasOrdenTrabajo(Orden.Id,1);
                await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);

            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }
        }
        private async Task VerRefaccionesTrabajo(int TrabajoID)
        {
            int trabajoId = TrabajoID;

            try
            {
                var refaccionesTrabajoPage = new RefaccionesTrabajo(trabajoId);
                await Application.Current.MainPage.Navigation.PushAsync(refaccionesTrabajoPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }
        }

        private async Task CargarDatosOrden()
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Iniciando carga de orden {_ordenId}");

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine("📡 Llamando API...");
                var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(_ordenId);

                if (ordenCompleta != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Orden cargada: {ordenCompleta.NumeroOrden}");
                    Orden = ordenCompleta;

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ ordenCompleta es NULL");
                    ErrorMessage = "No se pudo cargar la orden";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📚 StackTrace: {ex.StackTrace}");
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("🏁 Finalizó carga");
            }
        }

        private async Task VerCliente()
        {
            if (Orden == null) return;

            try
            {
                var clientePage = new ClientesPage(Orden.ClienteId);
                await Application.Current.MainPage.Navigation.PushAsync(clientePage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir los datos del cliente: {ex.Message}");
            }
        }

        private async Task VerVehiculo()
        {
            if (Orden == null) return;

            try
            {
                var vehiculoPage = new VehiculosPage(Orden.VehiculoId);
                await Application.Current.MainPage.Navigation.PushAsync(vehiculoPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir los datos del vehículo: {ex.Message}");
            }
        }

        private async Task VerEvidencias()
        {
            if (Orden == null) return;

            try
            {
                var evidenciasPage = new EvidenciasOrdenTrabajo(Orden.Id, 2);
                await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }

        }

        private async Task CancelarOrden()
        {
            if (Orden == null || !PuedeCancelar) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "⚠️ Cancelar Orden",
                $"¿Estás seguro de cancelar la orden {Orden.NumeroOrden}?\n\n" +
                "Esta acción cancelará todos los trabajos asociados.",
                "Sí, cancelar",
                "No");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var response = await _apiService.CancelarOrdenAsync(Orden.Id);

                if (response.Success)
                {
                    await MostrarAlerta("✅ Éxito", "Orden cancelada correctamente");
                    await CargarDatosOrden(); // Recargar datos
                }
                else
                {
                    await MostrarAlerta("❌ Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"Error al cancelar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EntregarOrden()
        {
            if (Orden == null || !PuedeEntregar) return;

            // Verificar progreso
            if (Orden.ProgresoGeneral < 100)
            {
                await MostrarAlerta(
                    "⚠️ No se puede entregar",
                    $"La orden aún no está completada.\n\n" +
                    $"Progreso actual: {Orden.ProgresoFormateado}\n" +
                    $"Trabajos: {Orden.ProgresoTexto}");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "🚗 Entregar Vehículo",
                $"¿Confirmas la entrega del vehículo?\n\n" +
                $"Orden: {Orden.NumeroOrden}\n" +
                $"Cliente: {Orden.ClienteNombre}\n" +
                $"Vehículo: {Orden.VehiculoCompleto}",
                "Sí, entregar",
                "Cancelar");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var response = await _apiService.EntregarOrdenAsync(Orden.Id);

                if (response.Success)
                {
                    await MostrarAlerta(
                        "✅ Vehículo Entregado",
                        "El vehículo ha sido entregado correctamente.\n" +
                        "Se ha registrado en el historial.");

                    // Regresar a la página anterior
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    await MostrarAlerta("❌ Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"Error al entregar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task MostrarAlerta(string titulo, string mensaje)
        {
            try
            {
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
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