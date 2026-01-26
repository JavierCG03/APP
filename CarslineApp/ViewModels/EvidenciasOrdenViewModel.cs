using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    public class EvidenciaOrdenViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private int _ordenGeneralId;
        private ObservableCollection<EvidenciaItem> _evidencias;
        private bool _isLoading;
        private string _mensajeEstado;

        // Nuevas propiedades para mostrar datos de la orden
        private string _numeroOrden;
        private string _vehiculoCompleto;
        private string _vin;

        public EvidenciaOrdenViewModel()
        {
            _apiService = new ApiService();
            _evidencias = new ObservableCollection<EvidenciaItem>();

            TomarFotoCommand = new Command<string>(async (tipo) => await TomarFoto(tipo));
            SeleccionarImagenCommand = new Command<string>(async (tipo) => await SeleccionarImagen(tipo));
            GuardarEvidenciasCommand = new Command(async () => await GuardarEvidencias(), () => Evidencias.Any(e => e.TieneImagen));
            EliminarEvidenciaCommand = new Command<EvidenciaItem>(async (evidencia) => await EliminarEvidencia(evidencia));
            CargarEvidenciasCommand = new Command(async () => await CargarEvidenciasExistentes());
            BackCommand = new Command(async () => await RegresarAtras());
            VerImagenCommand = new Command<ImageSource>(async (imagen) =>
            {
                if (imagen == null) return;

                await Application.Current.MainPage.Navigation
                    .PushModalAsync(new ImagePreviewPage(imagen));
            });

            InicializarTiposEvidencia();
        }

        #region Propiedades

        public int OrdenGeneralId
        {
            get => _ordenGeneralId;
            set
            {
                _ordenGeneralId = value;
                OnPropertyChanged();
                // Cargar detalles de la orden y evidencias existentes
                Task.Run(async () =>
                {
                    await InicializarDetallesOrden(value);
                    await CargarEvidenciasExistentes();
                });
            }
        }

        public string NumeroOrden
        {
            get => _numeroOrden;
            set
            {
                _numeroOrden = value;
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

        public ObservableCollection<EvidenciaItem> Evidencias
        {
            get => _evidencias;
            set
            {
                _evidencias = value;
                OnPropertyChanged();
                ((Command)GuardarEvidenciasCommand).ChangeCanExecute();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set
            {
                _mensajeEstado = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand TomarFotoCommand { get; }
        public ICommand SeleccionarImagenCommand { get; }
        public ICommand GuardarEvidenciasCommand { get; }
        public ICommand EliminarEvidenciaCommand { get; }
        public ICommand CargarEvidenciasCommand { get; }
        public ICommand VerImagenCommand { get; }
        public ICommand BackCommand { get; }

        #endregion

        #region Métodos Privados

        private void InicializarTiposEvidencia()
        {
            // Tipos predefinidos de evidencias
            var tipos = new[]
            {
                "Frontal",
                "Lateral Derecha",
                "Trasera",
                "Lateral Izquierda",
                "Interior Delantero",
                "Interior Trasero",
                "Tablero",
                "Llantas",
                "Motor",
                "Cajuela"
            };

            foreach (var tipo in tipos)
            {
                Evidencias.Add(new EvidenciaItem { Descripcion = tipo });
            }
        }

        private async Task InicializarDetallesOrden(int ordenId)
        {
            try
            {
                IsLoading = true;
                MensajeEstado = "Cargando información de la orden...";

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

                // Asignar los datos a las propiedades para binding
                NumeroOrden = ordenCompleta.NumeroOrden ?? "N/A";
                VehiculoCompleto = ordenCompleta.VehiculoCompleto ?? "Vehículo no especificado";
                VIN = ordenCompleta.VIN ?? "VIN no disponible";

                MensajeEstado = "Información de orden cargada correctamente";
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cargar detalle: {ex.Message}",
                    "OK");

                // Valores por defecto en caso de error
                NumeroOrden = "Error al cargar";
                VehiculoCompleto = "Error al cargar";
                VIN = "Error al cargar";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RegresarAtras()
        {
            try
            {
                // Verificar si hay cambios sin guardar
                var evidenciasSinGuardar = Evidencias.Any(e => e.TieneImagen && e.EvidenciaId == null);

                if (evidenciasSinGuardar)
                {
                    bool confirmar = await Application.Current.MainPage.DisplayAlert(
                        "Cambios sin guardar",
                        "Tienes evidencias sin guardar. ¿Deseas salir de todas formas?",
                        "Sí, salir",
                        "Cancelar");

                    if (!confirmar)
                        return;
                }

                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al regresar: {ex.Message}");
            }
        }

        private async Task TomarFoto(string tipo)
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = $"Capturar {tipo}"
                    });

                    if (photo != null)
                    {
                        await ProcesarImagen(photo, tipo);
                    }
                }
                else
                {
                    MensajeEstado = "La cámara no está disponible en este dispositivo";
                    await Application.Current.MainPage.DisplayAlert("Advertencia", "La cámara no está disponible", "OK");
                }
            }
            catch (PermissionException)
            {
                MensajeEstado = "Se necesitan permisos de cámara";
                await Application.Current.MainPage.DisplayAlert("Permisos", "Se necesitan permisos de cámara para continuar", "OK");
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al tomar foto: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al tomar foto: {ex.Message}", "OK");
            }
        }

        private async Task SeleccionarImagen(string tipo)
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = $"Seleccionar {tipo}"
                });

                if (photo != null)
                {
                    await ProcesarImagen(photo, tipo);
                }
            }
            catch (PermissionException)
            {
                MensajeEstado = "Se necesitan permisos de galería";
                await Application.Current.MainPage.DisplayAlert("Permisos", "Se necesitan permisos para acceder a la galería", "OK");
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al seleccionar imagen: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al seleccionar imagen: {ex.Message}", "OK");
            }
        }

        private async Task ProcesarImagen(FileResult photo, string tipo)
        {
            var evidencia = Evidencias.FirstOrDefault(e => e.Descripcion == tipo);
            if (evidencia != null)
            {
                // Leer la imagen como bytes
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                evidencia.ImagenBytes = memoryStream.ToArray();
                evidencia.NombreArchivo = photo.FileName;
                evidencia.TieneImagen = true;

                // Convertir a ImageSource para preview
                evidencia.ImagenPreview = ImageSource.FromStream(() => new MemoryStream(evidencia.ImagenBytes));

                MensajeEstado = $"Imagen {tipo} capturada correctamente";
                ((Command)GuardarEvidenciasCommand).ChangeCanExecute();
            }
        }



        private async Task EliminarEvidencia(EvidenciaItem evidencia)
        {
            try
            {
                if (evidencia == null || !evidencia.TieneImagen)
                    return;

                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Eliminar evidencia",
                    $"¿Estás seguro de eliminar la foto \"{evidencia.Descripcion}\"?",
                    "Sí, eliminar",
                    "Cancelar"
                );

                if (!confirmar)
                    return;

                IsLoading = true;
                MensajeEstado = "Eliminando evidencia...";

                // Si ya existe en servidor, eliminarla vía API
                if (evidencia.EvidenciaId > 0)
                {
                    var eliminar = await _apiService.EliminarEvidencia(evidencia.EvidenciaId.Value);

                    if (!eliminar.Success)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            eliminar.Message ?? "No se pudo eliminar la evidencia",
                            "OK");
                        return;
                    }
                }

                // 🧹 Limpiar evidencia local
                evidencia.ImagenBytes = null;
                evidencia.NombreArchivo = null;
                evidencia.TieneImagen = false;
                evidencia.ImagenPreview = null;
                evidencia.EvidenciaId = null;

                MensajeEstado = $"Evidencia {evidencia.Descripcion} eliminada";
                ((Command)GuardarEvidenciasCommand).ChangeCanExecute();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al eliminar evidencias: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task GuardarEvidencias()
        {
            try
            {
                IsLoading = true;
                MensajeEstado = "Guardando evidencias...";

                var evidenciasConImagen = Evidencias.Where(e => e.TieneImagen && e.ImagenBytes != null).ToList();

                if (!evidenciasConImagen.Any())
                {
                    MensajeEstado = "No hay evidencias nuevas para guardar";
                    await Application.Current.MainPage.DisplayAlert("Información", "No hay evidencias nuevas para guardar", "OK");
                    return;
                }

                // Preparar lista de evidencias para subir
                var evidenciasUpload = evidenciasConImagen.Select(e => new EvidenciaUpload
                {
                    Descripcion = e.Descripcion,
                    ImagenBytes = e.ImagenBytes,
                    NombreArchivo = e.NombreArchivo ?? $"{e.Descripcion.Replace(" ", "_")}.jpg"
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"OrdenGeneralId: {OrdenGeneralId}");
                System.Diagnostics.Debug.WriteLine($"Número de evidencias: {evidenciasConImagen.Count}");
                foreach (var ev in evidenciasConImagen)
                {
                    System.Diagnostics.Debug.WriteLine($"- {ev.Descripcion}: {ev.ImagenBytes?.Length ?? 0} bytes");
                }

                // Llamar al API Service
                var resultado = await _apiService.SubirEvidencias(OrdenGeneralId, evidenciasUpload);

                if (resultado.Success)
                {
                    MensajeEstado = resultado.Message;

                    // Limpiar evidencias después de guardar
                    foreach (var evidencia in evidenciasConImagen)
                    {
                        evidencia.ImagenBytes = null;
                        evidencia.NombreArchivo = null;
                        evidencia.TieneImagen = false;
                        evidencia.ImagenPreview = null;
                    }

                    await Application.Current.MainPage.DisplayAlert("Éxito", resultado.Message, "OK");

                    // Recargar evidencias desde el servidor
                    await CargarEvidenciasExistentes();
                }
                else
                {
                    MensajeEstado = resultado.Message;
                    await Application.Current.MainPage.DisplayAlert("Error", resultado.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al guardar evidencias: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                ((Command)GuardarEvidenciasCommand).ChangeCanExecute();
            }
        }

        private async Task CargarEvidenciasExistentes()
        {
            try
            {
                if (OrdenGeneralId <= 0)
                    return;

                IsLoading = true;
                MensajeEstado = "Cargando evidencias existentes...";

                var resultado = await _apiService.ObtenerEvidenciasPorOrden(OrdenGeneralId);

                if (resultado.Success && resultado.Data != null && resultado.Data.Any())
                {
                    // Marcar las evidencias que ya existen en el servidor
                    foreach (var evidenciaDto in resultado.Data)
                    {
                        var evidenciaLocal = Evidencias.FirstOrDefault(e => e.Descripcion == evidenciaDto.Descripcion);
                        if (evidenciaLocal != null)
                        {
                            evidenciaLocal.EvidenciaId = evidenciaDto.Id;
                            evidenciaLocal.TieneImagen = true;

                            // Cargar preview desde el servidor
                            var imagenBytes = await _apiService.ObtenerImagen(evidenciaDto.Id);
                            if (imagenBytes.Success && imagenBytes.Data != null)
                            {
                                evidenciaLocal.ImagenBytes = imagenBytes.Data;
                                evidenciaLocal.ImagenPreview = ImageSource.FromStream(() => new MemoryStream(imagenBytes.Data));
                            }
                        }
                    }

                    MensajeEstado = $"Se cargaron {resultado.Data.Count} evidencias existentes";
                }
                else if (!resultado.Success)
                {
                    // Si no hay evidencias, no es un error
                    MensajeEstado = "No hay evidencias previas para esta orden";
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar evidencias: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
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

    #region Clase EvidenciaItem

    public class EvidenciaItem : INotifyPropertyChanged
    {
        private int? _evidenciaId;
        private string _descripcion;
        private byte[] _imagenBytes;
        private string _nombreArchivo;
        private bool _tieneImagen;
        private ImageSource _imagenPreview;

        public int? EvidenciaId
        {
            get => _evidenciaId;
            set
            {
                _evidenciaId = value;
                OnPropertyChanged();
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                _descripcion = value;
                OnPropertyChanged();
            }
        }

        public byte[] ImagenBytes
        {
            get => _imagenBytes;
            set
            {
                _imagenBytes = value;
                OnPropertyChanged();
            }
        }

        public string NombreArchivo
        {
            get => _nombreArchivo;
            set
            {
                _nombreArchivo = value;
                OnPropertyChanged();
            }
        }

        public bool TieneImagen
        {
            get => _tieneImagen;
            set
            {
                _tieneImagen = value;
                OnPropertyChanged();
            }
        }

        public ImageSource ImagenPreview
        {
            get => _imagenPreview;
            set
            {
                _imagenPreview = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}