
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views.Buscador;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class AsesorMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private int _tipoOrdenSeleccionado = 1;
        private bool _isLoading;
        private string _nombreUsuarioActual = string.Empty;

        private ObservableCollection<OrdenDetalladaDto> _ordenesPendientes = new();
        private ObservableCollection<OrdenDetalladaDto> _ordenesProceso = new();
        private ObservableCollection<OrdenDetalladaDto> _ordenesFinalizadas = new();

        public AsesorMainViewModel()
        {
            _apiService = new ApiService();

            // Comandos de navegación
            VerServicioCommand = new Command(() => CambiarTipoOrden(1));
            VerDiagnosticoCommand = new Command(() => CambiarTipoOrden(2));
            VerReparacionCommand = new Command(() => CambiarTipoOrden(3));
            VerGarantiaCommand = new Command(() => CambiarTipoOrden(4));

            // Comandos de acciones
            CrearOrdenCommand = new Command(async () => await OnCrearOrden());
            CrearOrdenDesdeMenuCommand = new Command(async () => await OnCrearOrdenDesdeMenu());
            RefreshCommand = new Command(async () => await CargarOrdenes());
            CancelarOrdenCommand = new Command<int>(async (ordenId) => await CancelarOrden(ordenId));
            EntregarOrdenCommand = new Command<int>(async (ordenId) => await EntregarOrden(ordenId));
            LogoutCommand = new Command(async () => await OnLogout());
            BuscadorCommand = new Command(async () =>
            {
                try
                {
                    // Cerrar el menú lateral primero
                    if (Application.Current.MainPage is FlyoutPage flyoutPage)
                    {
                        flyoutPage.IsPresented = false;
                    }

                    await Task.Delay(300); // Dar tiempo a que se cierre el menú
                    await Application.Current.MainPage.Navigation.PushAsync(new BuscadorPage());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error navegación: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo abrir el buscador", "OK");
                }
            });
            TomarEvidenciasCommand = new Command<int>(async (ordenId) =>
            {
                var tomarEvidenciasPage = new EvidenciasOrdenPage(ordenId);
                await Application.Current.MainPage.Navigation.PushAsync(tomarEvidenciasPage);
            });


            VerDetalleOrdenCommand = new Command<int>(async (ordenId) => await NavegarAOrden(ordenId));
            //VerDetalleOrdenCommand = new Command<int>(async (ordenId) => await VerDetalleOrden(ordenId));


            NombreUsuarioActual = Preferences.Get("user_name", "Asesor");
        }

        /// <summary>
        /// Método público para inicializar desde la vista
        /// </summary>
        public async Task InicializarAsync()
        {
            await CargarOrdenes();
        }

        #region Propiedades

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set { _nombreUsuarioActual = value; OnPropertyChanged(); }
        }

        public int TipoOrdenSeleccionado
        {
            get => _tipoOrdenSeleccionado;
            set
            {
                _tipoOrdenSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloSeccion));
                OnPropertyChanged(nameof(EsServicio));
                OnPropertyChanged(nameof(EsDiagnostico));
                OnPropertyChanged(nameof(EsReparacion));
                OnPropertyChanged(nameof(EsGarantia));
            }
        }

        public string TituloSeccion => TipoOrdenSeleccionado switch
        {
            1 => "SERVICIOS",
            2 => "DIAGNÓSTICOS",
            3 => "REPARACIÓNES",
            4 => "GARANTÍAS",
            _ => "ÓRDENES"
        };

        public bool EsServicio => TipoOrdenSeleccionado == 1;
        public bool EsDiagnostico => TipoOrdenSeleccionado == 2;
        public bool EsReparacion => TipoOrdenSeleccionado == 3;
        public bool EsGarantia => TipoOrdenSeleccionado == 4;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OrdenDetalladaDto> OrdenesPendientes
        {
            get => _ordenesPendientes;
            set { _ordenesPendientes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OrdenDetalladaDto> OrdenesProceso
        {
            get => _ordenesProceso;
            set { _ordenesProceso = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OrdenDetalladaDto> OrdenesFinalizadas
        {
            get => _ordenesFinalizadas;
            set { _ordenesFinalizadas = value; OnPropertyChanged(); }
        }

        public bool HayPendientes => OrdenesPendientes.Any();
        public bool HayProceso => OrdenesProceso.Any();
        public bool HayFinalizadas => OrdenesFinalizadas.Any();

        #endregion

        #region Comandos

        public ICommand VerServicioCommand { get; }
        public ICommand VerDiagnosticoCommand { get; }
        public ICommand VerReparacionCommand { get; }
        public ICommand VerGarantiaCommand { get; }
        public ICommand CrearOrdenCommand { get; }
        public ICommand CrearOrdenDesdeMenuCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CancelarOrdenCommand { get; }
        public ICommand EntregarOrdenCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand VerDetalleOrdenCommand { get; } // ✅ NUEVO
        public ICommand TomarEvidenciasCommand { get; }
        public ICommand BuscadorCommand { get; }

        #endregion

        #region Métodos
        private async Task NavegarAOrden(int ordenId)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new OrdenPage(ordenId));
        }

        private async void CambiarTipoOrden(int tipoOrden)
        {
            TipoOrdenSeleccionado = tipoOrden;
            await CargarOrdenes();
        }

        /// <summary>
        /// ✅ ACTUALIZADO: Ahora trabaja con el nuevo modelo que incluye trabajos
        /// </summary>
        private async Task CargarOrdenes()
        {
            IsLoading = true;

            try
            {
                int asesorId = Preferences.Get("user_id", 9);
                System.Diagnostics.Debug.WriteLine($"🔄 Cargando órdenes - TipoOrden: {TipoOrdenSeleccionado}, AsesorId: {asesorId}");

                var ordenes = await _apiService.ObtenerOrdenesPorTipoAsync(TipoOrdenSeleccionado, asesorId);
                System.Diagnostics.Debug.WriteLine($"📦 Órdenes recibidas de API: {ordenes?.Count ?? 0}");

                // Ejecutar en el hilo principal de UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Limpiar colecciones
                    OrdenesPendientes.Clear();
                    OrdenesProceso.Clear();
                    OrdenesFinalizadas.Clear();

                    // Clasificar y agregar
                    if (ordenes != null && ordenes.Any())
                    {
                        foreach (var orden in ordenes)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"  📋 Orden {orden.NumeroOrden} - EstadoId: {orden.EstadoId} - " +
                                $"Trabajos: {orden.TrabajosCompletados}/{orden.TotalTrabajos} ({orden.ProgresoGeneral:F1}%)");

                            if (orden.EsPendiente)
                            {
                                OrdenesPendientes.Add(orden);
                                System.Diagnostics.Debug.WriteLine($"    ➡️ Agregada a PENDIENTES");
                            }
                            else if (orden.EsProceso)
                            {
                                OrdenesProceso.Add(orden);
                                System.Diagnostics.Debug.WriteLine($"    ➡️ Agregada a PROCESO");
                            }
                            else if (orden.EsFinalizada)
                            {
                                OrdenesFinalizadas.Add(orden);
                                System.Diagnostics.Debug.WriteLine($"    ➡️ Agregada a FINALIZADAS");
                            }
                        }
                    }
                    // Notificar cambios
                    NotificarCambiosDashboards();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR al cargar órdenes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cargar órdenes: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NotificarCambiosDashboards()
        {
            // Notificar las colecciones principales
            OnPropertyChanged(nameof(OrdenesPendientes));
            OnPropertyChanged(nameof(OrdenesProceso));
            OnPropertyChanged(nameof(OrdenesFinalizadas));

            // Notificar los indicadores booleanos
            OnPropertyChanged(nameof(HayPendientes));
            OnPropertyChanged(nameof(HayProceso));
            OnPropertyChanged(nameof(HayFinalizadas));
        }

        private async Task OnCrearOrden()
        {
            var crearOrdenPage = new CrearOrdenPage(TipoOrdenSeleccionado);
            await Application.Current.MainPage.Navigation.PushAsync(crearOrdenPage);

            // Recargar cuando regrese
            crearOrdenPage.Disappearing += async (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("🔄 Recargando órdenes después de crear...");
                await CargarOrdenes();
            };
        }

        private async Task OnCrearOrdenDesdeMenu()
        {
            try
            {
                var accion = await Application.Current.MainPage.DisplayActionSheet(
                    "Seleccionar tipo de orden",
                    "Cancelar",
                    null,
                    "🔧 Servicio",
                    "🔍 Diagnóstico",
                    "🛠️ Reparación",
                    "✅ Garantía"
                );

                if (accion == "Cancelar" || string.IsNullOrEmpty(accion))
                    return;

                // Determinar el tipo de orden según la selección
                int tipoOrden = accion switch
                {
                    "🔧 Servicio" => 1,
                    "🔍 Diagnóstico" => 2,
                    "🛠️ Reparación" => 3,
                    "✅ Garantía" => 4,
                    _ => 1
                };

                // Navegar a la página de crear orden
                var crearOrdenPage = new CrearOrdenPage(tipoOrden);
                await Application.Current.MainPage.Navigation.PushAsync(crearOrdenPage);

                // Recargar cuando regrese
                crearOrdenPage.Disappearing += async (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Recargando órdenes después de crear...");
                    await CargarOrdenes();
                };
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al abrir selector: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// ✅ NUEVO: Ver detalle completo de una orden con sus trabajos
        /// </summary>
        /*
        private async Task VerDetalleOrden(int ordenId)
        {
            try
            {
                IsLoading = true;

                // Obtener orden completa con trabajos
                var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(ordenId);

                if (ordenCompleta == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo cargar el detalle de la orden",
                        "OK");
                    return;
                }

                // Crear mensaje con detalles
                var mensaje = $"📋 {ordenCompleta.NumeroOrden}\n\n" +
                             $"Cliente: {ordenCompleta.ClienteNombre}\n" +
                             $"Telefono: {ordenCompleta.ClienteTelefono}\n" +
                             $"Vehículo: {ordenCompleta.VehiculoCompleto}\n" +
                             $"VIN: {ordenCompleta.VIN}\n\n" +
                             $"Promesa: {ordenCompleta.FechaHoraPromesaEntrega}\n" +
                             $"📊 Progreso: {ordenCompleta.ProgresoTexto} ({ordenCompleta.ProgresoFormateado})\n\n" +
                             $"🔧 TRABAJOS:\n";

                foreach (var trabajo in ordenCompleta.Trabajos)
                {
                    var icono = trabajo.EstadoTrabajo switch
                    {
                        1 => "⏳", // Pendiente
                        2 => "🛠️", // Asignado
                        3 => "🔨", // En Proceso
                        4 => "✅", // Completado
                        5 => "⏸️", // Pausado
                        6 => "❌", // Cancelado
                        _ => "📌"
                    };

                    mensaje += $"\n{icono}{trabajo.EstadoTrabajoNombre} \n-{trabajo.Trabajo} ";

                    if (!string.IsNullOrEmpty(trabajo.TecnicoNombre))
                        mensaje += $"\n   👨‍🔧 {trabajo.TecnicoNombre}";

                    if (!string.IsNullOrEmpty(trabajo.DuracionFormateada) && trabajo.DuracionFormateada != "-")
                        mensaje += $"\n   ⏱️ {trabajo.DuracionFormateada}";

                    mensaje += "\n";
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Detalle de Orden",
                    mensaje,
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cargar detalle: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }*/

        private async Task CancelarOrden(int ordenId)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cancelar Orden",
                "¿Estás seguro que deseas cancelar esta orden?\n\n" +
                "⚠️ Esto cancelará todos los trabajos pendientes.",
                "Sí",
                "No");

            if (!confirm) return;

            IsLoading = true;

            try
            {
                var response = await _apiService.CancelarOrdenAsync(ordenId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Orden cancelada correctamente",
                        "OK");

                    await CargarOrdenes();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cancelar orden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EntregarOrden(int ordenId)
        {
            try
            {
                IsLoading = true;

                // Obtener detalle de la orden para verificar trabajos
                var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(ordenId);

                if (ordenCompleta == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo verificar el estado de la orden",
                        "OK");
                    return;
                }

                // Verificar si hay trabajos sin completar
                var trabajosPendientes = ordenCompleta.Trabajos.Count(t => t.EstadoTrabajo != 4);

                if (trabajosPendientes > 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ No se puede entregar",
                        $"Hay {trabajosPendientes} trabajo(s) sin completar.\n\n" +
                        $"Progreso actual: {ordenCompleta.ProgresoFormateado}\n\n" +
                        "Todos los trabajos deben estar completados antes de entregar el vehículo.",
                        "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Entregar Vehículo",
                    $"✅ Todos los trabajos están completados.\n\n" +
                    $"¿Confirmas que el cliente recogió su vehículo?\n\n" +
                    $"Orden: {ordenCompleta.NumeroOrden}\n" +
                    $"Cliente: {ordenCompleta.ClienteNombre}\n" +
                    $"Vehículo: {ordenCompleta.VehiculoCompleto}",
                    "Sí, entregar",
                    "Cancelar");

                if (!confirm) return;

                var response = await _apiService.EntregarOrdenAsync(ordenId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Vehículo entregado correctamente.\n" +
                        "Se ha registrado en el historial.",
                        "OK");
                    await CargarOrdenes();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al entregar orden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnLogout()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "No");

            if (confirm)
            {
                Preferences.Clear();
                Application.Current.MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#512BD4"),
                    BarTextColor = Colors.White
                };
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}