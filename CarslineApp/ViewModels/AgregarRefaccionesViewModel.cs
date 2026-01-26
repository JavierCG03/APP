using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class AgregarRefaccionViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _numeroParte = string.Empty;
        private string _tipoRefaccion = string.Empty;
        private string _marcaVehiculo = string.Empty;
        private string _modelo = string.Empty;
        private string _ubicacion= string.Empty;
        private string _anio = string.Empty;
        private string _cantidad = string.Empty;
        private bool _mostrarOtroTipo;
        private string _otroTipoRefaccion = string.Empty;

        // Errores de validación
        private string _errorNumeroParte = string.Empty;
        private string _errorTipoRefaccion = string.Empty;
        private string _errorAnio = string.Empty;
        private string _errorCantidad = string.Empty;

        public List<string> TiposRefaccion { get; } = new()
        {
            "Filtro Aceite",
            "Filtro Aire Cabina",
            "Filtro Aire Motor",
            "Balatas delanteras",
            "Balatas traseras",
            "Bujias",
            "Neumaticos",
            "Otro"
        };

        public AgregarRefaccionViewModel()
        {
            _apiService = new ApiService();
            GuardarCommand = new Command(async () => await OnGuardar(), () => !IsLoading);
            CancelarCommand = new Command(async () => await OnCancelar());
        }

        #region Propiedades
        public bool MostrarOtroTipo
        {
            get => _mostrarOtroTipo;
            set { _mostrarOtroTipo = value; OnPropertyChanged(); }
        }

        public string OtroTipoRefaccion
        {
            get => _otroTipoRefaccion;
            set { _otroTipoRefaccion = value; OnPropertyChanged(); }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)GuardarCommand).ChangeCanExecute();
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

        public string NumeroParte
        {
            get => _numeroParte;
            set
            {
                _numeroParte = value;
                OnPropertyChanged();
                ErrorNumeroParte = string.Empty;
            }
        }

        public string TipoRefaccion
        {
            get => _tipoRefaccion;
            set
            {
                _tipoRefaccion = value;
                OnPropertyChanged();
                ErrorTipoRefaccion = string.Empty;

                // NUEVO: Mostrar campo "Otro" si es necesario
                MostrarOtroTipo = value == "Otro";
                if (!MostrarOtroTipo)
                {
                    OtroTipoRefaccion = string.Empty;
                }
            }
        }

        public string MarcaVehiculo
        {
            get => _marcaVehiculo;
            set
            {
                _marcaVehiculo = value;
                OnPropertyChanged();
            }
        }
        public string Ubicacion
        {
            get => _ubicacion;
            set
            {
                _ubicacion = value;
                OnPropertyChanged();
            }
        }
        public string Modelo
        {
            get => _modelo;
            set
            {
                _modelo = value;
                OnPropertyChanged();
            }
        }

        public string Anio
        {
            get => _anio;
            set
            {
                _anio = value;
                OnPropertyChanged();
                ErrorAnio = string.Empty;
            }
        }

        public string Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                ErrorCantidad = string.Empty;
            }
        }

        // Errores de validación
        public string ErrorNumeroParte
        {
            get => _errorNumeroParte;
            set
            {
                _errorNumeroParte = value;
                OnPropertyChanged();
            }
        }

        public string ErrorTipoRefaccion
        {
            get => _errorTipoRefaccion;
            set
            {
                _errorTipoRefaccion = value;
                OnPropertyChanged();
            }
        }

        public string ErrorAnio
        {
            get => _errorAnio;
            set
            {
                _errorAnio = value;
                OnPropertyChanged();
            }
        }

        public string ErrorCantidad
        {
            get => _errorCantidad;
            set
            {
                _errorCantidad = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        #endregion

        #region Métodos

        private bool ValidarFormulario()
        {
            bool esValido = true;
            ErrorMessage = string.Empty;

            // Validar Número de Parte
            if (string.IsNullOrWhiteSpace(NumeroParte))
            {
                ErrorNumeroParte = "El número de parte es obligatorio";
                esValido = false;
            }

            // Validar Tipo de Refacción
            if (string.IsNullOrWhiteSpace(TipoRefaccion))
            {
                ErrorTipoRefaccion = "Debe seleccionar un tipo de refacción";
                esValido = false;
            }
            // NUEVO: Validar "Otro" tipo
            if (TipoRefaccion == "Otro" && string.IsNullOrWhiteSpace(OtroTipoRefaccion))
            {
                ErrorTipoRefaccion = "Debe especificar el tipo de refacción";
                esValido = false;
            }

            // Validar Año (si se ingresó)
            if (!string.IsNullOrWhiteSpace(Anio))
            {
                if (!int.TryParse(Anio, out int anioInt))
                {
                    ErrorAnio = "El año debe ser un número válido";
                    esValido = false;
                }
                else if (anioInt < 1900 || anioInt > DateTime.Now.Year + 1)
                {
                    ErrorAnio = $"El año debe estar entre 1900 y {DateTime.Now.Year + 1}";
                    esValido = false;
                }
            }

            // Validar Cantidad
            if (string.IsNullOrWhiteSpace(Cantidad))
            {
                ErrorCantidad = "La cantidad es obligatoria";
                esValido = false;
            }
            else if (!int.TryParse(Cantidad, out int cantidadInt))
            {
                ErrorCantidad = "La cantidad debe ser un número válido";
                esValido = false;
            }
            else if (cantidadInt < 0)
            {
                ErrorCantidad = "La cantidad no puede ser negativa";
                esValido = false;
            }

            return esValido;
        }

        private async Task OnGuardar()
        {
            if (!ValidarFormulario())
            {
                ErrorMessage = "Por favor, corrija los errores antes de continuar";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                string tipoFinal = TipoRefaccion == "Otro" ? OtroTipoRefaccion : TipoRefaccion;

                var request = new CrearRefaccionRequest
                {
                    NumeroParte = NumeroParte.Trim(),
                    TipoRefaccion = tipoFinal,
                    MarcaVehiculo = string.IsNullOrWhiteSpace(MarcaVehiculo) ? null : MarcaVehiculo.Trim(),
                    Ubicacion = string.IsNullOrWhiteSpace(Ubicacion) ? null : Ubicacion.Trim(),
                    Modelo = string.IsNullOrWhiteSpace(Modelo) ? null : Modelo.Trim(),
                    Anio = string.IsNullOrWhiteSpace(Anio) ? null : int.Parse(Anio),
                    Cantidad = int.Parse(Cantidad)
                };

                var response = await _apiService.CrearRefaccionAsync(request);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Refacción agregada correctamente",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    ErrorMessage,
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnCancelar()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cancelar",
                "¿Está seguro de cancelar? Se perderán los datos ingresados",
                "Sí",
                "No");

            if (confirm)
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