using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<CrearOrdenResponse> CrearOrdenConTrabajosAsync(
        CrearOrdenConTrabajosRequest request,
        int asesorId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    $"{BaseUrl}/Ordenes/crear-con-trabajos")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add("X-User-Id", asesorId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CrearOrdenResponse>();
                    return result ?? new CrearOrdenResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta"
                    };
                }

                return new CrearOrdenResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new CrearOrdenResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO: Obtener órdenes por tipo (ahora retorna OrdenDetalladaDto con trabajos)
        /// </summary>
        public async Task<List<OrdenDetalladaDto>> ObtenerOrdenesPorTipoAsync(int tipoOrdenId, int asesorId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/Ordenes/asesor/{tipoOrdenId}");
                httpRequest.Headers.Add("X-User-Id", asesorId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    // El API ahora retorna OrdenConTrabajosDto[], lo convertimos a OrdenDetalladaDto[]
                    var ordenesCompletas = await response.Content.ReadFromJsonAsync<List<OrdenConTrabajosDto>>();

                    if (ordenesCompletas == null) return new List<OrdenDetalladaDto>();

                    // Mapear a OrdenDetalladaDto (simplificado para dashboard)
                    var ordenes = ordenesCompletas.Select(o => new OrdenDetalladaDto
                    {
                        Id = o.Id,
                        NumeroOrden = o.NumeroOrden,
                        VehiculoCompleto = o.VehiculoCompleto,
                        ClienteNombre = o.ClienteNombre,
                        ClienteTelefono = o.ClienteTelefono,
                        TipoServicio = o.TipoServicio,
                        HoraPromesa = o.FechaHoraPromesaEntrega.ToString("h:mm tt"),
                        FechaPromesa = o.FechaHoraPromesaEntrega.ToString("ddd/dd/MMM"),
                        HoraInicio = "-", // Se puede calcular del primer trabajo
                        HoraFin = o.HoraFin?.ToString("h:mm tt") ?? string.Empty,
                        NombreTecnico = o.Trabajos.FirstOrDefault(t => t.TecnicoNombre != null)?.TecnicoNombre ?? "-",
                        CostoTotal = o.CostoTotal,
                        EstadoId = o.EstadoOrdenId,
                        TotalTrabajos = o.TotalTrabajos,
                        TrabajosCompletados = o.TrabajosCompletados,
                        ProgresoGeneral = o.ProgresoGeneral,
                        TieneEvidencia = o.TieneEvidencia
                    }).ToList();

                    return ordenes;
                }
                return new List<OrdenDetalladaDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener órdenes: {ex.Message}");
                return new List<OrdenDetalladaDto>();
            }
        }
        public async Task<List<OrdenDetalladaDto>> ObtenerOrdenesPorTipo_JefeAsync(int tipoOrdenId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/Ordenes/Jefe-Taller/{tipoOrdenId}");

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    // El API ahora retorna OrdenConTrabajosDto[], lo convertimos a OrdenDetalladaDto[]
                    var ordenesCompletas = await response.Content.ReadFromJsonAsync<List<OrdenConTrabajosDto>>();

                    if (ordenesCompletas == null) return new List<OrdenDetalladaDto>();

                    // Mapear a OrdenDetalladaDto (simplificado para dashboard)
                    var ordenes = ordenesCompletas.Select(o => new OrdenDetalladaDto
                    {
                        Id = o.Id,
                        NumeroOrden = o.NumeroOrden,
                        VehiculoCompleto = o.VehiculoCompleto,
                        ClienteNombre = o.ClienteNombre,
                        ClienteTelefono = o.ClienteTelefono,
                        HoraPromesa = o.FechaHoraPromesaEntrega.ToString("HH:mm"),
                        HoraInicio = "-", // Se puede calcular del primer trabajo
                        HoraFin = o.HoraFin?.ToString("h:mm tt") ?? string.Empty,
                        NombreTecnico = o.Trabajos.FirstOrDefault(t => t.TecnicoNombre != null)?.TecnicoNombre ?? "-",
                        CostoTotal = o.CostoTotal,
                        EstadoId = o.EstadoOrdenId,
                        TotalTrabajos = o.TotalTrabajos,
                        TrabajosCompletados = o.TrabajosCompletados,
                        ProgresoGeneral = o.ProgresoGeneral
                    }).ToList();

                    return ordenes;
                }
                return new List<OrdenDetalladaDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener órdenes: {ex.Message}");
                return new List<OrdenDetalladaDto>();
            }
        }
        /// <summary>
        /// ✅ NUEVO: Obtener orden completa con todos sus trabajos
        /// </summary>
        public async Task<OrdenConTrabajosDto> ObtenerOrdenCompletaAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Ordenes/detalle/{ordenId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OrdenConTrabajosDto>();
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ✅ NUEVO: Obtener trabajos de una orden
        /// </summary>
        public async Task<List<TrabajoDto>> ObtenerTrabajosOrdenAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Trabajos/orden/{ordenId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<TrabajoDto>>();
                    return result ?? new List<TrabajoDto>();
                }

                return new List<TrabajoDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<TrabajoDto>();
            }
        }

        public async Task<AuthResponse> CancelarOrdenAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{BaseUrl}/Ordenes/cancelar/{ordenId}", null);

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

        public async Task<TrabajoResponse> EntregarOrdenAsync(int ordenId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{BaseUrl}/Ordenes/entregar/{ordenId}", null);

                // Leer siempre el contenido como TrabajoResponse
                var result = await response.Content.ReadFromJsonAsync<TrabajoResponse>();

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new TrabajoResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                // Si falla, devolver el mensaje que vino del API
                return result ?? new TrabajoResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new TrabajoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}
