using CarslineApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<AuthResponse> AsignarTecnicoAsync(int trabajoId, int tecnicoId, int jefeId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Put,
                    $"{BaseUrl}/Trabajos/{trabajoId}/asignar-tecnico/{tecnicoId}")
                {
                    Content = null
                };
                httpRequest.Headers.Add("X-User-Id", jefeId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return result ?? new AuthResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> ReasignarTecnicoAsync(int trabajoId, int nuevoTecnicoId, int jefeId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Put,
                    $"{BaseUrl}/Trabajos/{trabajoId}/reasignar-tecnico/{nuevoTecnicoId}")
                {
                    Content = null
                };
                httpRequest.Headers.Add("X-User-Id", jefeId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return result ?? new AuthResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<MisTrabajosResponseDto?> ObtenerMisTrabajosAsync(
           int tecnicoId,
           int? estadoFiltro = null)
        {
            try
            {
                var url = $"{BaseUrl}/Trabajos/mis-trabajos/{tecnicoId}";

                // Agregar filtro si viene
                if (estadoFiltro.HasValue)
                    url += $"?estadoFiltro={estadoFiltro.Value}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<MisTrabajosResponseDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ObtenerMisTrabajosAsync: {ex.Message}");
                return null;
            }
        }



        public async Task<TrabajoSimpleResponse> ObtenerInfoTrabajo(int trabajoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/trabajos/Trabajo/{trabajoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<TrabajoSimpleResponse>();
                    return result ?? new TrabajoSimpleResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                // Si el trabajo no existe o no tiene mano de obra
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new TrabajoSimpleResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado"
                    };
                }

                return new TrabajoSimpleResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener mano de obra: {ex.Message}");
                return new TrabajoSimpleResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<TrabajoResponse> IniciarTrabajoAsync(int trabajoId, int tecnicoId)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BaseUrl}/trabajos/iniciar/{trabajoId}"
                );

                // Header requerido por tu API
                request.Headers.Add("X-User-Id", tecnicoId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return new TrabajoResponse
                    {
                        Success = false,
                        Message = $"Error HTTP: {response.StatusCode}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<TrabajoResponse>();

                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Respuesta vacía del servidor"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<TrabajoResponse> PausarTrabajoAsync(
           int trabajoId,
           int tecnicoId,
           string motivo)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BaseUrl}/trabajos/Pausar/{trabajoId}"
                );

                // Header requerido por tu API
                request.Headers.Add("X-User-Id", tecnicoId.ToString());

                // 🔴 Body: string plano
                request.Content = JsonContent.Create(motivo);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new TrabajoResponse
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(error)
                            ? $"Error HTTP: {response.StatusCode}"
                            : error
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<TrabajoResponse>();

                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Respuesta vacía del servidor"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<TrabajoResponse> ReanudarTrabajoAsync(
        int trabajoId,
        int tecnicoId)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BaseUrl}/trabajos/Reanudar/{trabajoId}"
                );

                request.Headers.Add("X-User-Id", tecnicoId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new TrabajoResponse
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(error)
                            ? $"Error HTTP: {response.StatusCode}"
                            : error
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<TrabajoResponse>();

                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Respuesta vacía del servidor"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<TrabajoResponse> RestablecerTrabajoAsync(
        int trabajoId,
        int tecnicoId)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BaseUrl}/trabajos/restablecer-pendiente/{trabajoId}"
                );

                request.Headers.Add("X-User-Id", tecnicoId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new TrabajoResponse
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(error)
                            ? $"Error HTTP: {response.StatusCode}"
                            : error
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<TrabajoResponse>();

                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Respuesta vacía del servidor"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<TrabajoResponse> CompletarTrabajoAsync(
        int trabajoId,
        int tecnicoId,
        string? comentarios)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"{BaseUrl}/trabajos/completar/{trabajoId}"
                );

                // Header requerido por la API
                request.Headers.Add("X-User-Id", tecnicoId.ToString());

                // Body: string (puede ser null)
                if (!string.IsNullOrWhiteSpace(comentarios))
                {
                    request.Content = JsonContent.Create(comentarios);
                }
                else
                {
                    // El endpoint acepta null
                    request.Content = JsonContent.Create<string?>(null);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return new TrabajoResponse
                    {
                        Success = false,
                        Message = $"Error HTTP: {response.StatusCode}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<TrabajoResponse>();

                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Respuesta vacía del servidor"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<TrabajoSimpleDto>?> ObtenerTrabajosTecnicosAsync()
        {
            try
            {
                var url = $"{BaseUrl}/Ordenes/Trabajos";

                Console.WriteLine($"🌐 Llamando a: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📦 Response: {json}");

                var trabajos = JsonSerializer.Deserialize<List<TrabajoSimpleDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                Console.WriteLine($"✅ Se obtuvieron {trabajos?.Count ?? 0} trabajos");
                return trabajos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ObtenerTrabajosTecnicosAsync: {ex.Message}");
                return null;
            }
        }

    }

}