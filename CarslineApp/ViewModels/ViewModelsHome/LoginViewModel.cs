using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Services;
using CarslineApp.Views;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _nombreUsuario = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await OnLoginClicked(), () => !IsLoading);
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                _nombreUsuario = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)LoginCommand).ChangeCanExecute();
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

        public ICommand LoginCommand { get; }

        private async Task OnLoginClicked()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                ErrorMessage = "Por favor ingresa tu nombre de usuario";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor ingresa tu contrasena";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.LoginAsync(NombreUsuario, Password);

                if (response.Success && response.Usuario != null)
                {
                    // Guardar datos del usuario
                    Preferences.Set("auth_token", response.Token ?? string.Empty);
                    Preferences.Set("user_id", response.Usuario.Id);
                    Preferences.Set("user_name", response.Usuario.NombreCompleto);
                    Preferences.Set("user_username", response.Usuario.NombreUsuario);
                    Preferences.Set("user_role_id", response.Usuario.RolId);
                    Preferences.Set("user_role_name", response.Usuario.NombreRol);

                    // Navegar segun el rol (carga bajo demanda)
                    await NavigateByRole(response.Usuario.NombreRol);
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

        private async Task NavigateByRole(string rolNombre)
        {
            // Limpiar la pila de navegacion y navegar a la pagina correspondiente
            // Solo se carga la pagina necesaria, no todas
            Page targetPage = rolNombre switch
            {
                "Administrador" => new AdminHomePage(),
                "Asesor de servicio" => new AsesorHomePage(),
                "Jefe de Taller" => new JefeHomePage(),
                "Gerente" => new GerenteHomePage(),
                "Tecnico de mantenimiento" => new TecnicoHomePage(),
                _ => null
            };

            if (targetPage != null)
            {
                // Reemplazar toda la navegacion con la nueva pagina
                Application.Current.MainPage = new NavigationPage(targetPage)
                {
                    BarBackgroundColor = Color.FromArgb("#512BD4"),
                    BarTextColor = Colors.White
                };
            }
            else
            {
                ErrorMessage = "Rol no reconocido";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}