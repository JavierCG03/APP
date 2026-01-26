using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<HistorialGeneralResponse> ObtenerHistorialGeneralVehiculoAsync(int vehiculoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/Historial/Historial-General/{vehiculoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<HistorialGeneralResponse>();

                    return result ?? new HistorialGeneralResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta del servidor",
                        Historial = new List<HistorialOrdenDto>()
                    };
                }

                return new HistorialGeneralResponse
                {
                    Success = false,
                    Message = "No se pudo obtener el historial",
                    Historial = new List<HistorialOrdenDto>()
                };
            }
            catch (Exception ex)
            {
                return new HistorialGeneralResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Historial = new List<HistorialOrdenDto>()
                };
            }
        }

        public async Task<HistorialVehiculoResponse> ObtenerHistorialVehiculoAsync(int vehiculoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/Historial/Historial-Servicio/{vehiculoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<HistorialVehiculoResponse>();

                    return result ?? new HistorialVehiculoResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta del servidor",
                        Historial = new List<HistorialServicioDto>()
                    };
                }

                return new HistorialVehiculoResponse
                {
                    Success = false,
                    Message = "No se pudo obtener el historial",
                    Historial = new List<HistorialServicioDto>()
                };
            }
            catch (Exception ex)
            {
                return new HistorialVehiculoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Historial = new List<HistorialServicioDto>()
                };
            }
        }
    }
}