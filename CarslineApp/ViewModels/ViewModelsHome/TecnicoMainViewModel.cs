using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using CarslineApp.Views.ChecksList;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class TecnicoMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private int _estadoTrabajoSeleccionado = 2;
        private bool _isLoading;
        private string _nombreUsuarioActual = string.Empty;

        // ✅ LISTA ÚNICA AGRUPADA
        private ObservableCollection<GrupoTrabajos> _todosLosTrabajos = new();

        public TecnicoMainViewModel()
        {
            _apiService = new ApiService();

            // Comandos de navegación
            VerPendientesCommand = new Command(() => CambiarEstadoTrabajo(2));
            VerPausadosCommand = new Command(() => CambiarEstadoTrabajo(5));
            VerFinalizadosCommand = new Command(() => CambiarEstadoTrabajo(4));

            // Comandos de acciones
            RefreshCommand = new Command(async () => await CargarTrabajos());
            LogoutCommand = new Command(async () => await OnLogout());

            // Comandos para Trabajos
            IniciarTrabajoCommand = new Command<MiTrabajoDto>(async (trabajo) => await IniciarTrabajo(trabajo));
            ReanudarTrabajoCommand = new Command<MiTrabajoDto>(async (trabajo) => await ReanudarTrabajo(trabajo));

            NombreUsuarioActual = Preferences.Get("user_name", "Tecnico");
        }

        public async Task InicializarAsync()
        {
            await CargarTrabajos();
        }

        #region Propiedades

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set { _nombreUsuarioActual = value; OnPropertyChanged(); }
        }

        public int EstadoTrabajoSeleccionado
        {
            get => _estadoTrabajoSeleccionado;
            set
            {
                _estadoTrabajoSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloSeccion));
                OnPropertyChanged(nameof(EsPendiente));
                OnPropertyChanged(nameof(EsPausado));
                OnPropertyChanged(nameof(EsFinalizado));
                OnPropertyChanged(nameof(MensajeNoTrabajos));
            }
        }

        public string TituloSeccion => EstadoTrabajoSeleccionado switch
        {
            2 => "PENDIENTES",
            5 => "PAUSADOS",
            4 => "FINALIZADOS",
            _ => "TRABAJOS"
        };

        public bool EsPendiente => EstadoTrabajoSeleccionado == 2;
        public bool EsPausado => EstadoTrabajoSeleccionado == 5;
        public bool EsFinalizado => EstadoTrabajoSeleccionado == 4;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ✅ LISTA ÚNICA OBSERVABLE
        public ObservableCollection<GrupoTrabajos> TodosLosTrabajos
        {
            get => _todosLosTrabajos;
            set
            {
                _todosLosTrabajos = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoHayTrabajos));
            }
        }

        public bool NoHayTrabajos => !TodosLosTrabajos.Any(g => g.Any());

        public string MensajeNoTrabajos => EstadoTrabajoSeleccionado switch
        {
            2 => "📋 No tienes trabajos pendientes",
            5 => "⏸️ No tienes trabajos pausados",
            4 => "✅ No tienes trabajos finalizados",
            _ => "📋 No hay trabajos disponibles"
        };

        #endregion

        #region Comandos

        public ICommand VerPendientesCommand { get; }
        public ICommand VerPausadosCommand { get; }
        public ICommand VerFinalizadosCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand IniciarTrabajoCommand { get; }
        public ICommand ReanudarTrabajoCommand { get; }

        #endregion

        #region Métodos de Navegación

        private async Task NavegarACheckList(MiTrabajoDto trabajo)
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new CheckListServicioPage(
                    trabajo.Id,
                    trabajo.OrdenGeneralId,
                    trabajo.NumeroOrden,
                    trabajo.Trabajo,
                    trabajo.VehiculoCompleto,
                    trabajo.IndicacionesTrabajo,
                    trabajo.VIN
                )
            );
        }

        private async Task NavegarACheckListReparacion(MiTrabajoDto trabajo)
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new CheckListReparacion(
                    trabajo.Id,
                    trabajo.OrdenGeneralId,
                    trabajo.NumeroOrden,
                    trabajo.Trabajo,
                    trabajo.VehiculoCompleto,
                    trabajo.IndicacionesTrabajo,
                    trabajo.VIN
                )
            );
        }

        private async Task NavegarACheckListDiagnostico(MiTrabajoDto trabajo)
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new CheckListDiagnostico(
                    trabajo.Id,
                    trabajo.OrdenGeneralId,
                    trabajo.NumeroOrden,
                    trabajo.Trabajo,
                    trabajo.VehiculoCompleto,
                    trabajo.IndicacionesTrabajo,
                    trabajo.VIN
                )
            );
        }

        private async Task NavegarACheckListGarantia(MiTrabajoDto trabajo)
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new CheckListGarantia(
                    trabajo.Id,
                    trabajo.OrdenGeneralId,
                    trabajo.NumeroOrden,
                    trabajo.Trabajo,
                    trabajo.VehiculoCompleto,
                    trabajo.IndicacionesTrabajo,
                    trabajo.VIN
                )
            );
        }

        private async Task NavegarACheckListReacondicionamiento()
        {
            await Application.Current.MainPage.Navigation.PushAsync(
                new CheckListReacondicionamiento()
            );
        }

        #endregion

        #region Carga de Datos

        private async void CambiarEstadoTrabajo(int estadoTrabajo)
        {
            EstadoTrabajoSeleccionado = estadoTrabajo;
            await CargarTrabajos();
        }

        private async Task CargarTrabajos()
        {
            IsLoading = true;

            try
            {
                int tecnicoId = Preferences.Get("user_id", 0);

                var response = await _apiService.ObtenerMisTrabajosAsync(
                    tecnicoId,
                    EstadoTrabajoSeleccionado
                );

                if (response == null || response.Trabajos == null)
                {
                    TodosLosTrabajos.Clear();
                    return;
                }

                // ✅ AGRUPAR TRABAJOS POR TIPO
                var grupos = new ObservableCollection<GrupoTrabajos>();

                // Servicio
                var servicios = response.Trabajos.Where(t => t.TipoOrden == 1).ToList();
                if (servicios.Any())
                {
                    grupos.Add(new GrupoTrabajos("🔧 SERVICIO", "#FFF5F5", "#D60000", "#D60000", servicios));
                }

                // Diagnóstico
                var diagnosticos = response.Trabajos.Where(t => t.TipoOrden == 2).ToList();
                if (diagnosticos.Any())
                {
                    grupos.Add(new GrupoTrabajos("🧪 DIAGNÓSTICO", "#E3F2FD", "#2196F3", "#2196F3", diagnosticos));
                }

                // Reparación
                var reparaciones = response.Trabajos.Where(t => t.TipoOrden == 3).ToList();
                if (reparaciones.Any())
                {
                    grupos.Add(new GrupoTrabajos("🛠️ REPARACIÓN", "#FFF8E1", "#FFA000", "#FFA000", reparaciones));
                }

                // Garantía
                var garantias = response.Trabajos.Where(t => t.TipoOrden == 4).ToList();
                if (garantias.Any())
                {
                    grupos.Add(new GrupoTrabajos("🛡️ GARANTÍA", "#E8F5E9", "#4CAF50", "#4CAF50", garantias));
                }

                // Reacondicionamiento
                var reacondicionamientos = response.Trabajos.Where(t => t.TipoOrden == 5).ToList();
                if (reacondicionamientos.Any())
                {
                    grupos.Add(new GrupoTrabajos("♻️ REACONDICIONAMIENTO", "#F3E5F5", "#9C27B0", "#9C27B0", reacondicionamientos));
                }

                TodosLosTrabajos = grupos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error CargarTrabajos: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Acciones de Trabajo

        private async Task ReanudarTrabajo(MiTrabajoDto trabajo)
        {
            if (trabajo == null)
                return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Reanudar trabajo",
                $"¿Deseas reanudar el trabajo:\n{trabajo.VehiculoCompleto}\n{trabajo.Trabajo}?",
                "Reanudar",
                "Cancelar");

            if (!confirmar)
                return;

            try
            {
                IsLoading = true;

                int tecnicoId = Preferences.Get("user_id", 0);
                var response = await _apiService.ReanudarTrabajoAsync(trabajo.Id, tecnicoId);

                if (!response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                    return;
                }

                await NavegarSegunTipoTrabajo(trabajo);
                await CargarTrabajos();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task IniciarTrabajo(MiTrabajoDto trabajo)
        {
            if (trabajo == null)
                return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Iniciar trabajo",
                $"¿Deseas iniciar el trabajo:\n{trabajo.VehiculoCompleto}\n{trabajo.Trabajo}?",
                "Iniciar",
                "Cancelar");

            if (!confirmar)
                return;

            try
            {
                IsLoading = true;

                int tecnicoId = Preferences.Get("user_id", 0);
                var response = await _apiService.IniciarTrabajoAsync(trabajo.Id, tecnicoId);

                if (!response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                    return;
                }

                await NavegarSegunTipoTrabajo(trabajo);
                await CargarTrabajos();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavegarSegunTipoTrabajo(MiTrabajoDto trabajo)
        {
            // Si es SERVICIO específico
            if (trabajo.TipoOrden == 1 &&
                (trabajo.Trabajo == "1er Servicio" ||
                 trabajo.Trabajo == "2do Servicio" ||
                 trabajo.Trabajo == "3er Servicio" ||
                 trabajo.Trabajo == "Servicio Externo"))
            {
                await NavegarACheckList(trabajo);
                return;
            }

            // Otros tipos
            switch (trabajo.TipoOrden)
            {
                case 1:
                    await NavegarACheckListReparacion(trabajo);
                    break;
                case 2:
                    await NavegarACheckListDiagnostico(trabajo);
                    break;
                case 3:
                    await NavegarACheckListReparacion(trabajo);
                    break;
                case 4:
                    await NavegarACheckListGarantia(trabajo);
                    break;
                case 5:
                    await NavegarACheckListReacondicionamiento();
                    break;
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

    // ✅ CLASE PARA AGRUPAR TRABAJOS
    public class GrupoTrabajos : ObservableCollection<MiTrabajoDto>
    {
        public string Titulo { get; set; }
        public string Color { get; set; }
        public string BorderColor { get; set; }
        public string TextColor { get; set; }

        public GrupoTrabajos(string titulo, string color, string borderColor, string textColor, List<MiTrabajoDto> trabajos)
            : base(trabajos)
        {
            Titulo = titulo;
            Color = color;
            BorderColor = borderColor;
            TextColor = textColor;
        }
    }
}