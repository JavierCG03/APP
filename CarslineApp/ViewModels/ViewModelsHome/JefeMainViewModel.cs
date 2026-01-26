using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class JefeMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private int _tipoOrdenSeleccionado = 1;
        private bool _isLoading;
        private string _nombreUsuarioActual = string.Empty;

        // ✅ LISTA ÚNICA AGRUPADA
        private ObservableCollection<GrupoOrdenes> _todasLasOrdenesAgrupadas = new();

        public JefeMainViewModel()
        {
            _apiService = new ApiService();

            // Comandos de navegación
            VerServicioCommand = new Command(() => CambiarTipoOrden(1));
            VerDiagnosticoCommand = new Command(() => CambiarTipoOrden(2));
            VerReparacionCommand = new Command(() => CambiarTipoOrden(3));
            VerGarantiaCommand = new Command(() => CambiarTipoOrden(4));
            VerTableroCommand = new Command(async () => await VerTablero());

            // Comandos de acciones
            RefreshCommand = new Command(async () => await CargarOrdenes());
            LogoutCommand = new Command(async () => await OnLogout());

            // Comandos para técnicos
            AsignarTecnicoCommand = new Command<TrabajoDto>(async (trabajo) => await AsignarTecnico(trabajo));
            ReasignarTecnicoCommand = new Command<TrabajoDto>(async (trabajo) => await ReasignarTecnico(trabajo));

            NombreUsuarioActual = Preferences.Get("user_name", "Jefe de Taller");
        }

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
            1 => "SERVICIO",
            2 => "DIAGNÓSTICO",
            3 => "REPARACIÓN",
            4 => "GARANTÍA",
            _ => "ÓRDENES"
        };

        public bool EsServicio => TipoOrdenSeleccionado == 1;
        public bool EsDiagnostico => TipoOrdenSeleccionado == 2;
        public bool EsReparacion => TipoOrdenSeleccionado == 3;
        public bool EsGarantia => TipoOrdenSeleccionado == 4;
        public bool TableroActivo => VerTableroCommand != null;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ✅ LISTA ÚNICA OBSERVABLE
        public ObservableCollection<GrupoOrdenes> TodasLasOrdenesAgrupadas
        {
            get => _todasLasOrdenesAgrupadas;
            set
            {
                _todasLasOrdenesAgrupadas = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand VerServicioCommand { get; }
        public ICommand VerDiagnosticoCommand { get; }
        public ICommand VerReparacionCommand { get; }
        public ICommand VerGarantiaCommand { get; }
        public ICommand VerTableroCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AsignarTecnicoCommand { get; }
        public ICommand ReasignarTecnicoCommand { get; }

        #endregion

        #region Métodos
        private async Task VerTablero()
        {             
            IsLoading = true;
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new TableroPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo navegar al tablero: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async void CambiarTipoOrden(int tipoOrden)
        {
            TipoOrdenSeleccionado = tipoOrden;
            await CargarOrdenes();
        }

        private async Task CargarOrdenes()
        {
            IsLoading = true;

            try
            {
                // Obtener órdenes básicas
                var ordenesList = await _apiService.ObtenerOrdenesPorTipo_JefeAsync(TipoOrdenSeleccionado);
                System.Diagnostics.Debug.WriteLine($"📦 Órdenes recibidas: {ordenesList?.Count ?? 0}");

                if (ordenesList == null || !ordenesList.Any())
                {
                    TodasLasOrdenesAgrupadas = new ObservableCollection<GrupoOrdenes>();
                    return;
                }

                // Separar por estado
                var ordenesPendientes = new List<OrdenConTrabajosDto>();
                var ordenesProceso = new List<OrdenConTrabajosDto>();
                var ordenesFinalizadas = new List<OrdenConTrabajosDto>();

                foreach (var ordenBasica in ordenesList)
                {
                    var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(ordenBasica.Id);

                    if (ordenCompleta != null)
                    {
                        // ✅ Enriquecer la orden con propiedades de UI
                        EnriquecerOrden(ordenCompleta);

                        if (ordenCompleta.EstadoOrdenId == 1)
                            ordenesPendientes.Add(ordenCompleta);
                        else if (ordenCompleta.EstadoOrdenId == 2)
                            ordenesProceso.Add(ordenCompleta);
                        else if (ordenCompleta.EstadoOrdenId == 3)
                            ordenesFinalizadas.Add(ordenCompleta);
                    }
                }

                // ✅ CREAR GRUPOS
                var grupos = new ObservableCollection<GrupoOrdenes>();

                if (ordenesPendientes.Any())
                {
                    grupos.Add(new GrupoOrdenes(
                        "📋 ÓRDENES PENDIENTES",
                        "#FFE5E5",// (string titulo, string backgroundColor, string borderColor, string textColor
                        "#D60000",//
                        "Black",//
                        ordenesPendientes
                    ));
                }

                if (ordenesProceso.Any())
                {
                    grupos.Add(new GrupoOrdenes(
                        "⚙️ ÓRDENES EN PROCESO",
                        "#FFF8E1",
                        "#FFA500",
                        "#404040",
                        ordenesProceso
                    ));
                }

                if (ordenesFinalizadas.Any())
                {
                    grupos.Add(new GrupoOrdenes(
                        "✅ ÓRDENES FINALIZADAS",
                        "#F1F8F4",
                        "#4CAF50",
                        "#404040",
                        ordenesFinalizadas
                    ));
                }

                TodasLasOrdenesAgrupadas = grupos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
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

        /// <summary>
        /// Enriquece la orden con propiedades calculadas para la UI
        /// </summary>
        private void EnriquecerOrden(OrdenConTrabajosDto orden)
        {
            // Propiedades según el estado
            switch (orden.EstadoOrdenId)
            {
                case 1: // Pendiente
                    orden.BackgroundOrden = "#2A2A2A";
                    orden.BorderOrden = "White";
                    orden.TextOrden = "White";
                    orden.BadgeColor = "#D60000";
                    orden.TrabajosHeaderColor = "#D60000";
                    orden.MostrarProgreso = false;
                    break;

                case 2: // En Proceso
                    orden.BackgroundOrden = "#FFF8E1";
                    orden.BorderOrden = "#FFA500";
                    orden.TextOrden = "#1A1A1A";
                    orden.BadgeColor = "#FFA500";
                    orden.ProgresoBackground = "#FFFBF5";
                    orden.ProgresoColor = "#FFA500";
                    orden.TrabajosHeaderColor = "#FFA500";
                    orden.MostrarProgreso = true;
                    orden.MostrarBarraProgreso = true;
                    break;

                case 3: // Finalizada
                    orden.BackgroundOrden = "#F1F8F4";
                    orden.BorderOrden = "#4CAF50";
                    orden.TextOrden = "#1A1A1A";
                    orden.BadgeColor = "#4CAF50";
                    orden.ProgresoBackground = "#F9FFF9";
                    orden.ProgresoColor = "#4CAF50";
                    orden.TrabajosHeaderColor = "#4CAF50";
                    orden.MostrarProgreso = true;
                    orden.MostrarBarraProgreso = false;
                    break;
            }

            // Enriquecer trabajos
            if (orden.Trabajos != null)
            {
                foreach (var trabajo in orden.Trabajos)
                {
                    EnriquecerTrabajo(trabajo, orden.EstadoOrdenId);
                }
            }
        }

        /// <summary>
        /// Enriquece el trabajo con propiedades de UI
        /// </summary>
        private void EnriquecerTrabajo(TrabajoDto trabajo, int estadoOrden)
        {
            // Mostrar estado solo en Proceso y Finalizadas
            trabajo.MostrarEstado = estadoOrden == 2 || estadoOrden == 3;
            trabajo.NoMostrarEstado = estadoOrden == 1;

            // Determinar si tiene técnico
            trabajo.TieneTecnico = trabajo.TecnicoAsignadoId.HasValue;
        }

        #endregion

        #region Asignación de Técnicos

        private async Task AsignarTecnico(TrabajoDto trabajo)
        {
            try
            {
                IsLoading = true;

                var tecnicos = await _apiService.ObtenerTecnicosAsync();

                if (tecnicos == null || !tecnicos.Any())
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sin técnicos",
                        "No hay técnicos disponibles para asignar",
                        "OK");
                    return;
                }

                var nombresTecnicos = tecnicos.Select(t => t.NombreCompleto).ToArray();

                string tecnicoSeleccionado = await Application.Current.MainPage.DisplayActionSheet(
                    $"Asignar técnico a:\n{trabajo.Trabajo}",
                    "Cancelar",
                    null,
                    nombresTecnicos);

                if (string.IsNullOrEmpty(tecnicoSeleccionado) || tecnicoSeleccionado == "Cancelar")
                    return;

                var tecnico = tecnicos.FirstOrDefault(t => t.NombreCompleto == tecnicoSeleccionado);
                if (tecnico == null) return;

                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar asignación",
                    $"¿Asignar a {tecnico.NombreCompleto}?\n\nTrabajo: {trabajo.Trabajo}",
                    "Sí, asignar",
                    "Cancelar");

                if (!confirmar) return;

                int jefeId = Preferences.Get("user_id", 0);
                var response = await _apiService.AsignarTecnicoAsync(trabajo.Id, tecnico.Id, jefeId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        $"Trabajo asignado a {tecnico.NombreCompleto}",
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
                    $"Error al asignar técnico: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReasignarTecnico(TrabajoDto trabajo)
        {
            try
            {
                if (trabajo.EnProceso)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "No disponible",
                        "No se puede reasignar un trabajo que ya está en proceso",
                        "OK");
                    return;
                }

                var tecnicos = await _apiService.ObtenerTecnicosAsync();

                if (tecnicos == null || !tecnicos.Any())
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sin técnicos",
                        "No hay técnicos disponibles",
                        "OK");
                    return;
                }

                var tecnicosDisponibles = tecnicos
                    .Where(t => t.Id != trabajo.TecnicoAsignadoId)
                    .ToList();

                if (!tecnicosDisponibles.Any())
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Sin opciones",
                        "No hay otros técnicos disponibles para reasignar",
                        "OK");
                    return;
                }

                var nombresTecnicos = tecnicosDisponibles.Select(t => t.NombreCompleto).ToArray();

                string nuevoTecnico = await Application.Current.MainPage.DisplayActionSheet(
                    $"Reasignar trabajo:\n{trabajo.Trabajo}\n\nTécnico actual: {trabajo.TecnicoNombre}",
                    "Cancelar",
                    null,
                    nombresTecnicos);

                if (string.IsNullOrEmpty(nuevoTecnico) || nuevoTecnico == "Cancelar")
                    return;

                var tecnico = tecnicosDisponibles.FirstOrDefault(t => t.NombreCompleto == nuevoTecnico);
                if (tecnico == null) return;

                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar reasignación",
                    $"Cambiar de:\n{trabajo.TecnicoNombre}\n\nA:\n{tecnico.NombreCompleto}\n\nTrabajo: {trabajo.Trabajo}",
                    "Sí, reasignar",
                    "Cancelar");

                if (!confirmar) return;

                int jefeId = Preferences.Get("user_id", 0);
                var response = await _apiService.ReasignarTecnicoAsync(trabajo.Id, tecnico.Id, jefeId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        $"Trabajo reasignado a {tecnico.NombreCompleto}",
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
                    $"Error al reasignar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

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
                    BarBackgroundColor = Color.FromArgb("#D60000"),
                    BarTextColor = Colors.White
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ✅ CLASE PARA AGRUPAR ÓRDENES
    public class GrupoOrdenes : ObservableCollection<OrdenConTrabajosDto>
    {
        public string Titulo { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public string TextColor { get; set; }

        public GrupoOrdenes(string titulo, string backgroundColor, string borderColor, string textColor, List<OrdenConTrabajosDto> ordenes)
            : base(ordenes)
        {
            Titulo = titulo;
            BackgroundColor = backgroundColor;
            BorderColor = borderColor;
            TextColor = textColor;
        }
    }
}