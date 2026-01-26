using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class AdminHomeViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _nombreUsuarioActual = string.Empty;
        private bool _isLoading;
        private ObservableCollection<RolDto> _rolesDisponibles = new();
        private RolDto? _rolSeleccionado;
        private string _nuevoNombreCompleto = string.Empty;
        private string _nuevoNombreUsuario = string.Empty;
        private string _nuevaPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public AdminHomeViewModel()
        {
            _apiService = new ApiService();

            // Comandos del menú lateral
            VerUsuariosCommand = new Command(async () => await OnVerUsuarios());
            VerInventarioCommand = new Command(async () => await OnVerInventario());
            LogoutCommand = new Command(async () => await OnLogout());

            // Comando del formulario
            CrearUsuarioCommand = new Command(async () => await OnCrearUsuario(), () => !IsLoading);

            CargarDatosIniciales();
        }

        #region Propiedades

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set
            {
                _nombreUsuarioActual = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)CrearUsuarioCommand).ChangeCanExecute();
            }
        }

        public ObservableCollection<RolDto> RolesDisponibles
        {
            get => _rolesDisponibles;
            set
            {
                _rolesDisponibles = value;
                OnPropertyChanged();
            }
        }

        public RolDto? RolSeleccionado
        {
            get => _rolSeleccionado;
            set
            {
                _rolSeleccionado = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string NuevoNombreCompleto
        {
            get => _nuevoNombreCompleto;
            set
            {
                _nuevoNombreCompleto = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string NuevoNombreUsuario
        {
            get => _nuevoNombreUsuario;
            set
            {
                _nuevoNombreUsuario = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string NuevaPassword
        {
            get => _nuevaPassword;
            set
            {
                _nuevaPassword = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand CrearUsuarioCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand VerUsuariosCommand { get; }
        public ICommand VerInventarioCommand { get; }

        #endregion

        #region Métodos Privados

        private async void CargarDatosIniciales()
        {
            NombreUsuarioActual = Preferences.Get("user_name", "Administrador");
            await CargarRoles();
        }

        private async Task CargarRoles()
        {
            try
            {
                IsLoading = true;
                var roles = await _apiService.ObtenerRolesAsync();
                RolesDisponibles.Clear();
                foreach (var rol in roles)
                {
                    RolesDisponibles.Add(rol);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar roles: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnCrearUsuario()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.Navigation.PushAsync(new CrearUsuarioPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir el inventario: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnVerUsuarios()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.Navigation.PushAsync(new ListaUsuariosPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir la lista de usuarios: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnVerInventario()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.Navigation.PushAsync(new InventarioPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir el inventario: {ex.Message}",
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
                    BarBackgroundColor = Color.FromArgb("#D60000"),
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