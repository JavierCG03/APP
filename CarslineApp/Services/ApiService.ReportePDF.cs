using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        /// <summary>
        /// Descargar PDF de una orden
        /// </summary>
        public async Task<DescargarPdfResponse> DescargarPdfOrdenAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Pdf/orden/{ordenId}/descargar");

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = $"Orden_{ordenId}_{DateTime.Now:yyyyMMdd}.pdf";

                    return new DescargarPdfResponse
                    {
                        Success = true,
                        Message = "PDF descargado exitosamente",
                        PdfBytes = pdfBytes,
                        NombreArchivo = fileName
                    };
                }

                return new DescargarPdfResponse
                {
                    Success = false,
                    Message = $"Error al descargar PDF: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new DescargarPdfResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<PdfPreviewResponse> ObtenerVistaPreviaPdfAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{BaseUrl}/Pdf/orden/{ordenId}/preview", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PdfPreviewResponse>();
                    return result ?? new PdfPreviewResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new PdfPreviewResponse
                {
                    Success = false,
                    Message = $"Error al obtener vista previa: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new PdfPreviewResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}