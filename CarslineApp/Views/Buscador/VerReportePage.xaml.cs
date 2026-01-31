using System.Net;
using CarslineApp.Services;

namespace CarslineApp.Views.Buscador;

public partial class VerReportePage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isLoading;
    private int _ordenId;

    public VerReportePage(int ordenId)
    {
        InitializeComponent();
        _ordenId = ordenId;
        _apiService = new ApiService(); 

#if ANDROID
        ConfigurarWebViewAndroid();
#endif

        _ = CargarPdfAsync();
    }

#if ANDROID
    private void ConfigurarWebViewAndroid()
    {
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("pdfviewer", (handler, View) =>
        {
            handler.PlatformView.Settings.AllowFileAccess = true;
            handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
            handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;
            handler.PlatformView.Settings.JavaScriptEnabled = true;
        });
    }
#endif

    private async Task CargarPdfAsync()
    {
        try
        {
            _isLoading = true;

            // Mostrar indicador de carga (opcional)
            LoadingIndicator.IsVisible = true;

            // Llamar al servicio API para obtener el PDF
            var response = await _apiService.ObtenerVistaPreviaPdfAsync(_ordenId);

            if (!response.Success || string.IsNullOrEmpty(response.PdfBase64))
            {
                await DisplayAlert("Error", response.Message ?? "No se pudo obtener el PDF", "OK");
                return;
            }

            // Convertir el Base64 a bytes
            byte[] pdfBytes = Convert.FromBase64String(response.PdfBase64);

            // Determinar el nombre del archivo
            string fileName = string.IsNullOrEmpty(response.NumeroOrden)
                ? $"Orden_{_ordenId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                : $"Orden_{response.NumeroOrden}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // Guardar y mostrar el PDF según la plataforma
            await GuardarYMostrarPdfAsync(pdfBytes, fileName);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar el PDF: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Error en CargarPdfAsync: {ex}");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task GuardarYMostrarPdfAsync(byte[] pdfBytes, string fileName)
    {
#if ANDROID
        await GuardarYMostrarPdfAndroidAsync(pdfBytes, fileName);
#elif IOS || MACCATALYST
        await GuardarYMostrarPdfIOSAsync(pdfBytes, fileName);
#elif WINDOWS
        await GuardarYMostrarPdfWindowsAsync(pdfBytes, fileName);
#else
        await GuardarYMostrarPdfGenericoAsync(pdfBytes, fileName);
#endif
    }

#if ANDROID
    private async Task GuardarYMostrarPdfAndroidAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            // Guardar en el directorio de caché interno de la app
            string cachePath = FileSystem.Current.CacheDirectory;
            string filePath = Path.Combine(cachePath, fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            System.Diagnostics.Debug.WriteLine($"PDF guardado en: {filePath}");
            
            // Usar PDF.js para mostrar el PDF
            string encodedFileName = WebUtility.UrlEncode(fileName);
            
            // Opción 1: Si el PDF está en la carpeta de caché
            string pdfUrl = $"file://{filePath}";
            string viewerUrl = $"file:///android_asset/pdfjs/web/viewer.html?file={WebUtility.UrlEncode(pdfUrl)}";
            
            pdfview.Source = viewerUrl;
            
            System.Diagnostics.Debug.WriteLine($"Mostrando PDF desde: {viewerUrl}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en GuardarYMostrarPdfAndroidAsync: {ex}");
            await DisplayAlert("Error", $"No se pudo mostrar el PDF en Android: {ex.Message}", "OK");
        }
    }
#endif

#if IOS || MACCATALYST
    private async Task GuardarYMostrarPdfIOSAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            string cachePath = FileSystem.Current.CacheDirectory;
            string filePath = Path.Combine(cachePath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // iOS puede mostrar PDFs nativamente en el WebView
            pdfview.Source = filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en GuardarYMostrarPdfIOSAsync: {ex}");
            await DisplayAlert("Error", $"No se pudo mostrar el PDF en iOS: {ex.Message}", "OK");
        }
    }
#endif

#if WINDOWS
    private async Task GuardarYMostrarPdfWindowsAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            string cachePath = FileSystem.Current.CacheDirectory;
            string filePath = Path.Combine(cachePath, fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            // Windows: usar URI absoluta
            pdfview.Source = new Uri(filePath).AbsoluteUri;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en GuardarYMostrarPdfWindowsAsync: {ex}");
            await DisplayAlert("Error", $"No se pudo mostrar el PDF en Windows: {ex.Message}", "OK");
        }
    }
#endif

    private async Task GuardarYMostrarPdfGenericoAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            string cachePath = FileSystem.Current.CacheDirectory;
            string filePath = Path.Combine(cachePath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            pdfview.Source = filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en GuardarYMostrarPdfGenericoAsync: {ex}");
            await DisplayAlert("Error", $"No se pudo mostrar el PDF: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ = LimpiarArchivosTemporalesAsync();
    }

    private async Task LimpiarArchivosTemporalesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                string folderPath = FileSystem.Current.CacheDirectory;
                var files = Directory.GetFiles(folderPath, "Orden_*.pdf");

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        // Eliminar archivos más antiguos de 24 horas
                        if (DateTime.Now - fileInfo.CreationTime > TimeSpan.FromHours(24))
                        {
                            File.Delete(file);
                            System.Diagnostics.Debug.WriteLine($"Archivo temporal eliminado: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al eliminar archivo {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar archivos temporales: {ex.Message}");
            }
        });
    }
}