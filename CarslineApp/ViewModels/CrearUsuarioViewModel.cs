using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class CrearUsuarioViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private ObservableCollection<RolDto> _rolesDisponibles = new();
        private RolDto? _rolSeleccionado;
        private string _nuevoNombreCompleto = string.Empty;
        private string _nuevoNombreUsuario = string.Empty;
        private string _nuevaPassword = string.Empty;
        private string _repetirPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isPasswordVisible = false;
        private bool _isRepeatPasswordVisible = false;

        public CrearUsuarioViewModel()
        {
            _apiService = new ApiService();

            // Comandos
            CrearUsuarioCommand = new Command(async () => await OnCrearUsuario(), () => !IsLoading);
            VolverCommand = new Command(async () => await OnVolver());
            TogglePasswordVisibilityCommand = new Command(() => IsPasswordVisible = !IsPasswordVisible);
            ToggleRepeatPasswordVisibilityCommand = new Command(() => IsRepeatPasswordVisible = !IsRepeatPasswordVisible);

            CargarRoles();
        }

        #region Propiedades

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
                if (_rolSeleccionado == value) return;
                _rolSeleccionado = value;
                OnPropertyChanged();
                LimpiarMensajes();
            }
        }

        public string NuevoNombreCompleto
        {
            get => _nuevoNombreCompleto;
            set
            {
                _nuevoNombreCompleto = value;
                OnPropertyChanged();
                LimpiarMensajes();
            }
        }

        public string NuevoNombreUsuario
        {
            get => _nuevoNombreUsuario;
            set
            {
                _nuevoNombreUsuario = value;
                OnPropertyChanged();
                LimpiarMensajes();
            }
        }

        public string NuevaPassword
        {
            get => _nuevaPassword;
            set
            {
                _nuevaPassword = value;
                OnPropertyChanged();
                LimpiarMensajes();
            }
        }

        public string RepetirPassword
        {
            get => _repetirPassword;
            set
            {
                _repetirPassword = value;
                OnPropertyChanged();
                LimpiarMensajes();
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

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswordIcon));
            }
        }

        public bool IsRepeatPasswordVisible
        {
            get => _isRepeatPasswordVisible;
            set
            {
                _isRepeatPasswordVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RepeatPasswordIcon));
            }
        }

        public string PasswordIcon => IsPasswordVisible ? "eye_off.png" : "eye.png";
        public string RepeatPasswordIcon => IsRepeatPasswordVisible ? "eye_off.png" : "eye.png";

        #endregion

        #region Comandos

        public ICommand CrearUsuarioCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleRepeatPasswordVisibilityCommand { get; }

        #endregion

        #region Métodos Privados

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

        private void LimpiarMensajes()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private async Task OnCrearUsuario()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(NuevoNombreCompleto))
            {
                ErrorMessage = "Por favor ingresa el nombre completo";
                return;
            }

            if (string.IsNullOrWhiteSpace(NuevoNombreUsuario))
            {
                ErrorMessage = "Por favor ingresa el nombre de usuario";
                return;
            }

            if (string.IsNullOrWhiteSpace(NuevaPassword))
            {
                ErrorMessage = "Por favor ingresa la contraseña";
                return;
            }

            if (NuevaPassword.Length < 8)
            {
                ErrorMessage = "La contraseña debe tener al menos 8 caracteres";
                return;
            }

            if (string.IsNullOrWhiteSpace(RepetirPassword))
            {
                ErrorMessage = "Por favor repite la contraseña";
                return;
            }

            if (NuevaPassword != RepetirPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden";
                return;
            }

            if (RolSeleccionado == null)
            {
                ErrorMessage = "Por favor selecciona un tipo de usuario";
                return;
            }

            IsLoading = true;
            LimpiarMensajes();

            try
            {
                int adminId = Preferences.Get("user_id", 0);

                var response = await _apiService.CrearUsuarioAsync(
                    NuevoNombreCompleto,
                    NuevoNombreUsuario,
                    NuevaPassword,
                    RolSeleccionado.Id,
                    adminId);

                if (response.Success)
                {
                    SuccessMessage = "✅ Usuario creado exitosamente";

                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "El usuario ha sido creado correctamente",
                        "OK");

                    // Limpiar formulario
                    LimpiarFormulario();
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo crear el usuario: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LimpiarFormulario()
        {
            NuevoNombreCompleto = string.Empty;
            NuevoNombreUsuario = string.Empty;
            NuevaPassword = string.Empty;
            RepetirPassword = string.Empty;
            RolSeleccionado = null;
            IsPasswordVisible = false;
            IsRepeatPasswordVisible = false;
        }

        private async Task OnVolver()
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
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