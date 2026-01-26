using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views.Buscador;

namespace CarslineApp.ViewModels
{
    public class BuscadorViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private CancellationTokenSource _searchCts;

        #region Campos Privados

        private string _terminoBusqueda = string.Empty;
        private bool _isLoading = false;
        private bool _mostrarResultados = false;
        private string _mensajeError = string.Empty;
        private ObservableCollection<ResultadoBusquedaDto> _resultados = new();

        #endregion

        #region Propiedades

        public string TerminoBusqueda
        {
            get => _terminoBusqueda;
            set
            {
                _terminoBusqueda = value;
                OnPropertyChanged();
                MensajeError = string.Empty;

                // Búsqueda con debounce
                _ = BusquedaConDebounce();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)BuscarCommand).ChangeCanExecute();
            }
        }

        public bool MostrarResultados
        {
            get => _mostrarResultados;
            set
            {
                _mostrarResultados = value;
                OnPropertyChanged();
            }
        }

        public string MensajeError
        {
            get => _mensajeError;
            set
            {
                _mensajeError = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ResultadoBusquedaDto> Resultados
        {
            get => _resultados;
            set
            {
                _resultados = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand BuscarCommand { get; }
        public ICommand SeleccionarResultadoCommand { get; }
        public ICommand LimpiarBusquedaCommand { get; }

        #endregion

        #region Constructor

        public BuscadorViewModel()
        {
            try
            {
                _apiService = new ApiService();

                BuscarCommand = new Command(async () => await RealizarBusqueda(), () => !IsLoading);
                SeleccionarResultadoCommand = new Command<ResultadoBusquedaDto>(async (resultado) => await SeleccionarResultado(resultado));
                LimpiarBusquedaCommand = new Command(LimpiarResultados);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor: {ex.Message}");
                MensajeError = "Error al inicializar el buscador";
            }
        }

        #endregion

        #region Métodos de Búsqueda

        /// <summary>
        /// Búsqueda con debounce (espera 500ms después de escribir)
        /// </summary>
        private async Task BusquedaConDebounce()
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(800, _searchCts.Token);

                if (!string.IsNullOrWhiteSpace(TerminoBusqueda) && TerminoBusqueda.Length >= 3)
                {
                    await RealizarBusqueda();
                }
                else if (string.IsNullOrWhiteSpace(TerminoBusqueda))
                {
                    LimpiarResultados();
                }
            }
            catch (TaskCanceledException)
            {
                // Búsqueda cancelada (usuario sigue escribiendo)
            }
        }

        /// <summary>
        /// Realizar búsqueda unificada
        /// </summary>
        private async Task RealizarBusqueda()
        {
            if (string.IsNullOrWhiteSpace(TerminoBusqueda) || TerminoBusqueda.Length < 3)
            {
                MensajeError = "Ingresa al menos 3 caracteres";
                return;
            }

            IsLoading = true;
            MensajeError = string.Empty;
            Resultados.Clear();
            MostrarResultados = false;

            try
            {
                var response = await _apiService.BusquedaUnificadaAsync(TerminoBusqueda);

                if (response.Success && response.Resultados != null && response.Resultados.Any())
                {
                    Resultados.Clear();
                    foreach (var resultado in response.Resultados)
                    {
                        Resultados.Add(resultado);
                    }

                    MostrarResultados = true;
                }
                else
                {
                    MensajeError = response.Message ?? "No se encontraron resultados";
                    MostrarResultados = false;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
                MostrarResultados = false;
                System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Seleccionar resultado y navegar
        /// </summary>
        private async Task SeleccionarResultado(ResultadoBusquedaDto resultado)
        {
            if (resultado == null) return;

            try
            {
                IsLoading = true;

                switch (resultado.Tipo)
                {
                    case TipoResultadoBusqueda.Cliente:
                        await NavegarACliente(resultado.Id);
                        break;

                    case TipoResultadoBusqueda.Vehiculo:
                        await NavegarAVehiculo(resultado.Id);
                        break;

                    case TipoResultadoBusqueda.Orden:
                        await NavegarAOrden(resultado.Id);
                        break;

                    default:
                        await MostrarAlerta("⚠️", "Tipo no reconocido", "OK");
                        break;
                }
            }
            catch (Exception ex)
            {
                await MostrarAlerta("❌ Error", $"Error al navegar: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"❌ Error navegación: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Limpiar resultados
        /// </summary>
        private void LimpiarResultados()
        {
            TerminoBusqueda = string.Empty;
            Resultados.Clear();
            MostrarResultados = false;
            MensajeError = string.Empty;
        }

        #endregion

        #region Navegación

        private async Task NavegarACliente(int clienteId)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ClientesPage(clienteId));
        }

        private async Task NavegarAVehiculo(int vehiculoId)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new VehiculosPage(vehiculoId));
        }

        private async Task NavegarAOrden(int ordenId)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new OrdenPage(ordenId));
        }

        #endregion

        #region Auxiliares

        private static async Task MostrarAlerta(string titulo, string mensaje, string boton)
        {
            try
            {
                await Application.Current?.MainPage?.DisplayAlert(titulo, mensaje, boton);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error alerta: {ex.Message}");
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