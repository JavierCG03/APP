using CarslineApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<List<RefaccionDto>> ObtenerTodasRefaccionesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Refacciones/todos");

                if (!response.IsSuccessStatusCode)
                    return new List<RefaccionDto>();

                return await response.Content
                    .ReadFromJsonAsync<List<RefaccionDto>>() ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<RefaccionesPaginadasResponse?> ObtenerRefaccionesAsync(
            int pagina = 1,
            int porPagina = 20,
            string? busqueda = null)
        {
            try
            {
                var url = $"{BaseUrl}/Refacciones/paginado" +
                          $"?pagina={pagina}&porPagina={porPagina}";

                if (!string.IsNullOrWhiteSpace(busqueda))
                    url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content
                    .ReadFromJsonAsync<RefaccionesPaginadasResponse>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error API: {ex.Message}");
                return null;
            }
        }

        // ✅ NUEVO: Buscar refacción por número de parte
        public async Task<RefaccionDto?> BuscarPorNumeroParteAsync(string numeroParte)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Refacciones/buscar/{numeroParte}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefaccionDto>();
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al buscar refacción: {ex.Message}");
                return null;
            }
        }

        public async Task<RefaccionResponse> CrearRefaccionAsync(CrearRefaccionRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Refacciones/crear", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                    return result ?? new RefaccionResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                var errorContent = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                return errorContent ?? new RefaccionResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new RefaccionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<RefaccionResponse> AumentarCantidadAsync(int refaccionId, int cantidad)
        {
            try
            {
                var response = await _httpClient.PutAsync(
                    $"{BaseUrl}/Refacciones/aumentar/{refaccionId}/{cantidad}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                    return result ?? new RefaccionResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                var errorContent = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                return errorContent ?? new RefaccionResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new RefaccionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<RefaccionResponse> DisminuirCantidadAsync(int refaccionId, int cantidad)
        {
            try
            {
                var response = await _httpClient.PutAsync(
                    $"{BaseUrl}/Refacciones/disminuir/{refaccionId}/{cantidad}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                    return result ?? new RefaccionResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                var errorContent = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                return errorContent ?? new RefaccionResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new RefaccionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<RefaccionResponse> EliminarRefaccionAsync(int refaccionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/Refacciones/{refaccionId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RefaccionResponse>();
                    return result ?? new RefaccionResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new RefaccionResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new RefaccionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // En ApiService.cs - Agregar estos métodos

        // ✅ AGREGAR LOGS en el backend para depurar
        public async Task<RefaccionesPaginadasResponse?> ObtenerRefaccionesPaginadasAsync(
            int pagina = 1,
            int porPagina = 20,
            string? busqueda = null)
        {
            try
            {
                var url = $"{BaseUrl}/Refacciones/paginado" +
                          $"?pagina={pagina}&porPagina={porPagina}";

                if (!string.IsNullOrWhiteSpace(busqueda))
                    url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

                Debug.WriteLine($"🌐 Llamando: {url}");

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"📦 Respuesta: {json.Substring(0, Math.Min(200, json.Length))}...");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content
                    .ReadFromJsonAsync<RefaccionesPaginadasResponse>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error API: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RefaccionDto>> BusquedaRapidaAsync(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return new List<RefaccionDto>();

                var url = $"api/Refacciones/buscar-rapido?termino={Uri.EscapeDataString(termino)}";
                var response = await _httpClient.GetFromJsonAsync<List<RefaccionDto>>(url);
                return response ?? new List<RefaccionDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BusquedaRapidaAsync: {ex.Message}");
                return new List<RefaccionDto>();
            }
        }
    }
}