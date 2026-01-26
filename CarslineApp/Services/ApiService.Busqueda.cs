using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {

        /// <summary>
        /// Búsqueda unificada inteligente
        /// </summary>
        public async Task<BusquedaUnificadaResponse> BusquedaUnificadaAsync(string termino)
        {
            try
            {
                var url = $"{BaseUrl}/Busqueda/unificada?termino={Uri.EscapeDataString(termino)}";
                var response = await _httpClient.GetFromJsonAsync<BusquedaUnificadaResponse>(url);
                return response ?? new BusquedaUnificadaResponse
                {
                    Success = false,
                    Message = "No se pudo realizar la búsqueda"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda unificada: {ex.Message}");
                return new BusquedaUnificadaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Buscar orden por número de orden
        /// </summary>
        public async Task<BuscarOrdenResponse> BuscarOrdenPorNumeroAsync(string numeroOrden)
        {
            try
            {
                var url = $"{BaseUrl}/Busqueda/orden/{Uri.EscapeDataString(numeroOrden)}";
                var response = await _httpClient.GetFromJsonAsync<BuscarOrdenResponse>(url);
                return response ?? new BuscarOrdenResponse
                {
                    Success = false,
                    Message = "Orden no encontrada"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar orden: {ex.Message}");
                return new BuscarOrdenResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

    }
 }