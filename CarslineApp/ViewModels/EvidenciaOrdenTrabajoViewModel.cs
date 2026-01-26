using CarslineApp.Services;
using CarslineApp.Views;
using CarslineApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    public class EvidenciaOrdenTrabajoViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _yaInicializado = false;
        private ScrollView _carouselScrollView;

        private int _ordenGeneralId;
        private int _tipoEvidencia;
        private ObservableCollection<EvidenciaVisualizadorItem> _evidencias;
        private bool _isLoading;
        private bool _isLoadingImages;
        public bool _esServicio;
        private string _numeroOrden;
        private string _vehiculoCompleto;
        private string _vin;
        private string _servicio;
        private int _imagenesProgreso;
        private int _imagenesTotales;
        private string _titulo;
        private bool _mostrarBotonAnterior;
        private bool _mostrarBotonSiguiente;
        private int _indiceActual;

        // Constantes para la navegación
        private const double ANCHO_TARJETA = 280;
        private const double ESPACIADO = 15;
        private const double PADDING_LATERAL = 15;

        public EvidenciaOrdenTrabajoViewModel()
        {
            _apiService = new ApiService();
            _evidencias = new ObservableCollection<EvidenciaVisualizadorItem>();

            VerImagenCommand = new Command<EvidenciaVisualizadorItem>(async (item) => await AbrirPrevisualizador(item));
            BackCommand = new Command(async () => await RegresarAtras());
            ScrollAnteriorCommand = new Command(async () => await ScrollAnterior());
            ScrollSiguienteCommand = new Command(async () => await ScrollSiguiente());

            // Inicialmente ocultar botones
            MostrarBotonAnterior = false;
            MostrarBotonSiguiente = false;
        }

        #region Propiedades

        public int OrdenGeneralId
        {
            get => _ordenGeneralId;
            set
            {
                if (_ordenGeneralId == value) return;
                System.Diagnostics.Debug.WriteLine($"🔄 OrdenGeneralId cambiando de {_ordenGeneralId} a {value}");
                _ordenGeneralId = value;
                OnPropertyChanged();

                if (!_yaInicializado && value > 0)
                {
                    _ = InicializarAsync();
                }
            }
        }

        public int TipoEvidencia
        {
            get => _tipoEvidencia;
            set
            {
                if (_tipoEvidencia == value) return;
                _tipoEvidencia = value;
                OnPropertyChanged();
            }
        }

        public string NumeroOrden
        {
            get => _numeroOrden;
            set { _numeroOrden = value; OnPropertyChanged(); }
        }

        public string VehiculoCompleto
        {
            get => _vehiculoCompleto;
            set { _vehiculoCompleto = value; OnPropertyChanged(); }
        }

        public string VIN
        {
            get => _vin;
            set { _vin = value; OnPropertyChanged(); }
        }

        public string Titulo
        {
            get => _titulo;
            set { _titulo = value; OnPropertyChanged(); }
        }

        public string Servicio
        {
            get => _servicio;
            set { _servicio = value; OnPropertyChanged(); }
        }

        public bool EsServicio
        {
            get => _esServicio;
            set { _esServicio = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EvidenciaVisualizadorItem> Evidencias
        {
            get => _evidencias;
            set
            {
                _evidencias = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneEvidencias));
                OnPropertyChanged(nameof(NoTieneEvidencias));
                ActualizarIndicadorPosicion();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsLoadingImages
        {
            get => _isLoadingImages;
            set { _isLoadingImages = value; OnPropertyChanged(); }
        }

        public int ImagenesProgreso
        {
            get => _imagenesProgreso;
            set { _imagenesProgreso = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgresoTexto)); OnPropertyChanged(nameof(ProgresoValor)); }
        }

        public int ImagenesTotales
        {
            get => _imagenesTotales;
            set { _imagenesTotales = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgresoTexto)); OnPropertyChanged(nameof(ProgresoValor)); }
        }

        public bool MostrarBotonAnterior
        {
            get => _mostrarBotonAnterior;
            set { _mostrarBotonAnterior = value; OnPropertyChanged(); }
        }

        public bool MostrarBotonSiguiente
        {
            get => _mostrarBotonSiguiente;
            set { _mostrarBotonSiguiente = value; OnPropertyChanged(); }
        }

        public int IndiceActual
        {
            get => _indiceActual;
            set
            {
                _indiceActual = value;
                OnPropertyChanged();
                ActualizarIndicadorPosicion();
            }
        }

        public string IndicadorPosicion => Evidencias?.Count > 0 ? $"{IndiceActual + 1} / {Evidencias.Count}" : "";

        public string ProgresoTexto => $"Cargando imágenes {ImagenesProgreso}/{ImagenesTotales}...";

        public double ProgresoValor => ImagenesTotales > 0 ? (double)ImagenesProgreso / ImagenesTotales : 0;

        public bool TieneEvidencias => Evidencias?.Any() ?? false;
        public bool NoTieneEvidencias => !TieneEvidencias;

        #endregion

        #region Comandos

        public ICommand VerImagenCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ScrollAnteriorCommand { get; }
        public ICommand ScrollSiguienteCommand { get; }

        #endregion

        #region Métodos de Navegación del Carrusel

        public void SetScrollView(ScrollView scrollView)
        {
            _carouselScrollView = scrollView;
        }

        public void ActualizarEstadoBotones(double scrollX)
        {
            if (_carouselScrollView == null || Evidencias == null || !Evidencias.Any())
            {
                MostrarBotonAnterior = false;
                MostrarBotonSiguiente = false;
                return;
            }

            // Mostrar botón anterior si no estamos al inicio
            MostrarBotonAnterior = scrollX > 10;

            // Calcular si hay más contenido a la derecha
            var anchoContenido = (ANCHO_TARJETA + ESPACIADO) * Evidencias.Count;
            var anchoVisible = _carouselScrollView.Width;
            var scrollMaximo = anchoContenido - anchoVisible + (PADDING_LATERAL * 2);

            MostrarBotonSiguiente = scrollX < (scrollMaximo - 10);

            // Calcular índice actual aproximado
            var nuevoIndice = (int)Math.Round(scrollX / (ANCHO_TARJETA + ESPACIADO));
            if (nuevoIndice != IndiceActual && nuevoIndice >= 0 && nuevoIndice < Evidencias.Count)
            {
                IndiceActual = nuevoIndice;
            }
        }

        private async Task ScrollAnterior()
        {
            if (_carouselScrollView == null || IndiceActual <= 0) return;

            var nuevoIndice = Math.Max(0, IndiceActual - 1);
            await ScrollAIndice(nuevoIndice);
        }

        private async Task ScrollSiguiente()
        {
            if (_carouselScrollView == null || Evidencias == null || IndiceActual >= Evidencias.Count - 1) return;

            var nuevoIndice = Math.Min(Evidencias.Count - 1, IndiceActual + 1);
            await ScrollAIndice(nuevoIndice);
        }

        private async Task ScrollAIndice(int indice)
        {
            if (_carouselScrollView == null) return;

            try
            {
                var scrollX = indice * (ANCHO_TARJETA + ESPACIADO);
                await _carouselScrollView.ScrollToAsync(scrollX, 0, true);
                IndiceActual = indice;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al hacer scroll: {ex.Message}");
            }
        }

        private void ActualizarIndicadorPosicion()
        {
            OnPropertyChanged(nameof(IndicadorPosicion));
        }

        #endregion

        #region Métodos Principales

        private async Task InicializarAsync()
        {
            if (!await _semaphore.WaitAsync(0))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Ya hay una inicialización en curso, ignorando...");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 Inicializando orden {OrdenGeneralId}");

                IsLoading = true;
                _yaInicializado = true;

                await CargarDetallesOrden();
                await CargarEvidenciasBase();

                IsLoading = false;

                await CargarImagenesProgresivamente();

                // Actualizar estado inicial de los botones después de cargar
                if (_carouselScrollView != null && Evidencias?.Any() == true)
                {
                    await Task.Delay(500); // Esperar a que el layout se complete
                    ActualizarEstadoBotones(0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inicialización: {ex.Message}");
                await MostrarError("Error al inicializar", ex.Message);
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release();
            }
        }

        private async Task CargarDetallesOrden()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📋 Cargando detalles orden {OrdenGeneralId}");

                var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(OrdenGeneralId);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (ordenCompleta != null)
                        {
                            if (TipoEvidencia == 1)
                            {
                                Titulo = "🔧 Evidencias de Trabajo";
                            }
                            else
                            {
                                Titulo = "🔧 Evidencias de Recepcion";
                            }
                            if (ordenCompleta.TipoOrdenId == 1)
                            {
                                EsServicio = true;
                            }
                            else
                            {
                                EsServicio = false;
                            }
                            NumeroOrden = ordenCompleta.NumeroOrden ?? "N/A";
                            VehiculoCompleto = ordenCompleta.VehiculoCompleto ?? "Vehículo no especificado";
                            VIN = ordenCompleta.VIN ?? "VIN no disponible";
                            Servicio = ordenCompleta.TipoServicio ?? "No especificado";
                            System.Diagnostics.Debug.WriteLine($"✅ Detalles cargados: {NumeroOrden}");
                        }
                        else
                        {
                            NumeroOrden = VehiculoCompleto = VIN = Servicio = "Error al cargar";
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error en MainThread al asignar detalles: {ex.Message}");
                        NumeroOrden = VehiculoCompleto = VIN = Servicio = "Error al cargar";
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error detalles: {ex.Message}");
                throw;
            }
        }

        private async Task CargarEvidenciasBase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📦 Cargando lista de evidencias...");

                ApiResponse<List<EvidenciaDto>> resultado = null;

                if (TipoEvidencia == 1)
                {
                    resultado = await _apiService.ObtenerEvidenciasTrabajoPorOrden(OrdenGeneralId);
                }
                else if (TipoEvidencia == 2)
                {
                    resultado = await _apiService.ObtenerEvidenciasPorOrden(OrdenGeneralId);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        Evidencias.Clear();

                        if (resultado != null && resultado.Success && resultado.Data != null && resultado.Data.Any())
                        {
                            ImagenesTotales = resultado.Data.Count;

                            foreach (var evidenciaDto in resultado.Data)
                            {
                                try
                                {
                                    var item = new EvidenciaVisualizadorItem
                                    {
                                        EvidenciaId = evidenciaDto.Id,
                                        Descripcion = evidenciaDto.Descripcion ?? "Sin descripción",
                                        FechaRegistro = evidenciaDto.FechaRegistro,
                                        EstaCargando = true,
                                        TieneImagen = false
                                    };

                                    Evidencias.Add(item);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Error al crear item de evidencia {evidenciaDto.Id}: {ex.Message}");
                                }
                            }

                            System.Diagnostics.Debug.WriteLine($"✅ {Evidencias.Count} evidencias listadas");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron evidencias");
                            ImagenesTotales = 0;
                        }

                        OnPropertyChanged(nameof(TieneEvidencias));
                        OnPropertyChanged(nameof(NoTieneEvidencias));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error en MainThread al procesar evidencias: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar evidencias: {ex.Message}");
                throw;
            }
        }

        private async Task CargarImagenesProgresivamente()
        {
            try
            {
                if (!Evidencias.Any())
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No hay evidencias para cargar imágenes");
                    return;
                }

                IsLoadingImages = true;
                ImagenesProgreso = 0;

                System.Diagnostics.Debug.WriteLine($"🖼️ Iniciando carga progresiva de {Evidencias.Count} imágenes");

                var semaforo = new SemaphoreSlim(3, 3);
                var tareas = Evidencias.Select(evidencia => CargarImagenIndividual(evidencia, semaforo)).ToList();

                await Task.WhenAll(tareas);

                System.Diagnostics.Debug.WriteLine($"✅ Todas las imágenes procesadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en carga progresiva: {ex.Message}");
            }
            finally
            {
                IsLoadingImages = false;
            }
        }

        private async Task CargarImagenIndividual(EvidenciaVisualizadorItem item, SemaphoreSlim semaforo)
        {
            await semaforo.WaitAsync();
            try
            {
                if (item == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Item es null, saltando...");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"📸 Cargando imagen {item.EvidenciaId}...");

                var resultado = await _apiService.ObtenerImagen(item.EvidenciaId);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (resultado != null && resultado.Success && resultado.Data != null && resultado.Data.Length > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"🎯 Procesando imagen {item.EvidenciaId} en UI thread ({resultado.Data.Length} bytes)");

                            item.ImagenBytes = resultado.Data;
                            var imageSource = ImageSource.FromStream(() => new MemoryStream(resultado.Data));
                            item.ImagenPreview = imageSource;

                            Task.Delay(50).Wait();

                            item.TieneImagen = true;
                            item.EstaCargando = false;

                            System.Diagnostics.Debug.WriteLine($"✅ Imagen {item.EvidenciaId} asignada correctamente");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Sin imagen para {item.EvidenciaId}");
                            item.ImagenPreview = null;
                            item.TieneImagen = false;
                            item.EstaCargando = false;
                        }

                        ImagenesProgreso++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error al asignar imagen {item.EvidenciaId} en UI: {ex.Message}");
                        item.ImagenPreview = null;
                        item.TieneImagen = false;
                        item.EstaCargando = false;
                        ImagenesProgreso++;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error cargando imagen {item?.EvidenciaId}: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (item != null)
                        {
                            item.ImagenPreview = null;
                            item.TieneImagen = false;
                            item.EstaCargando = false;
                            ImagenesProgreso++;
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error en cleanup de imagen: {innerEx.Message}");
                    }
                });
            }
            finally
            {
                semaforo.Release();
            }
        }

        private async Task AbrirPrevisualizador(EvidenciaVisualizadorItem item)
        {
            try
            {
                if (item == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Item es null");
                    return;
                }

                if (!item.TieneImagen)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Item no tiene imagen");
                    await MostrarError("Sin imagen", "Esta evidencia no tiene imagen disponible");
                    return;
                }

                if (item.ImagenPreview == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ ImagenPreview es null");
                    await MostrarError("Error", "No se pudo cargar la imagen");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🖼️ Abriendo previsualizador para imagen {item.EvidenciaId}");

                await Application.Current.MainPage.Navigation.PushModalAsync(
                    new ImagePreviewPage(item.ImagenPreview));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al abrir previsualizador: {ex.Message}");
                await MostrarError("Error", "No se pudo abrir la imagen");
            }
        }

        private async Task RegresarAtras()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⬅️ Regresando atrás...");

                if (Application.Current?.MainPage?.Navigation != null)
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al regresar: {ex.Message}");
            }
        }

        private async Task MostrarError(string titulo, string mensaje)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error mostrando alerta: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en MostrarError: {ex.Message}");
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