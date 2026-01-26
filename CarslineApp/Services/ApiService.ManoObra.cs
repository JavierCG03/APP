using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        /// <summary>
        /// Fijar o actualizar el costo de mano de obra de un trabajo
        /// </summary>
        public async Task<ManoObraResponse> FijarCostoManoObraAsync(
            int trabajoId,
            decimal costoManoObra)
        {
            try
            {
                var request = new FijarManoObraRequest
                {
                    CostoManoObra = costoManoObra
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/Trabajos/FijarCostoManoObra/{trabajoId}",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ManoObraResponse>();
                    return result ?? new ManoObraResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                // Intentar leer el mensaje de error del servidor
                var errorContent = await response.Content.ReadFromJsonAsync<ManoObraResponse>();
                return errorContent ?? new ManoObraResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al fijar mano de obra: {ex.Message}");
                return new ManoObraResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener el costo de mano de obra de un trabajo
        /// </summary>
        public async Task<ManoObraResponse> ObtenerCostoManoObraAsync(int trabajoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/Trabajos/CostoManoObra/{trabajoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ManoObraResponse>();
                    return result ?? new ManoObraResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                // Si el trabajo no existe o no tiene mano de obra
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ManoObraResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    };
                }

                return new ManoObraResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener mano de obra: {ex.Message}");
                return new ManoObraResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}