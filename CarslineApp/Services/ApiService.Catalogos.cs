using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<List<TipoServicioDto>> ObtenerTiposServicioAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Catalogos/tipos-servicio");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<TipoServicioDto>>();
                    return result ?? new List<TipoServicioDto>();
                }
                return new List<TipoServicioDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener tipos de servicio: {ex.Message}");
                return new List<TipoServicioDto>();
            }
        }

        public async Task<List<ServicioExtraDto>> ObtenerServiciosFrecuentesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Catalogos/servicios-frecuentes");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<ServicioExtraDto>>();
                    return result ?? new List<ServicioExtraDto>();
                }
                return new List<ServicioExtraDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener servicios frecuentes: {ex.Message}");
                return new List<ServicioExtraDto>();
            }
        }

    }
}