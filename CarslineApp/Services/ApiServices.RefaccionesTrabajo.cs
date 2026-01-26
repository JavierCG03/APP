using CarslineApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {

            /// <summary>
            /// Agregar múltiples refacciones a un trabajo
            /// </summary>
        public async Task<AgregarRefaccionesResponse> AgregarRefaccionesTrabajo(
                AgregarRefaccionesTrabajoRequest request)
            {
                try
                {
                    Debug.WriteLine($"📤 Agregando {request.Refacciones.Count} refacciones al trabajo {request.TrabajoId}");

                    var response = await _httpClient.PostAsJsonAsync(
                        $"{BaseUrl}/RefaccionesTrabajo/agregar",
                        request);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content
                            .ReadFromJsonAsync<AgregarRefaccionesResponse>();

                        Debug.WriteLine($"✅ Refacciones agregadas exitosamente. Total: ${result?.TotalRefacciones:N2}");

                        return result ?? new AgregarRefaccionesResponse
                        {
                            Success = false,
                            Message = "Error al procesar la respuesta"
                        };
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ Error HTTP {response.StatusCode}: {errorContent}");

                    return new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = $"Error en la solicitud: {response.StatusCode}"
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Excepción en AgregarRefaccionesTrabajo: {ex.Message}");
                    return new AgregarRefaccionesResponse
                    {
                        Success = false,
                        Message = $"Error: {ex.Message}"
                    };
                }
            }

            /// <summary>
            /// Obtener todas las refacciones de un trabajo
            /// </summary>
        public async Task<ObtenerRefaccionesTrabajoResponse> ObtenerRefaccionesPorTrabajo(
                int trabajoId)
            {
                try
                {
                    Debug.WriteLine($"📥 Obteniendo refacciones del trabajo {trabajoId}");

                    var response = await _httpClient.GetAsync(
                        $"{BaseUrl}/RefaccionesTrabajo/trabajo/{trabajoId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content
                            .ReadFromJsonAsync<ObtenerRefaccionesTrabajoResponse>();

                        Debug.WriteLine($"✅ Se obtuvieron {result?.Refacciones.Count ?? 0} refacciones");

                        return result ?? new ObtenerRefaccionesTrabajoResponse
                        {
                            Success = false,
                            Message = "Error al procesar la respuesta",
                            Refacciones = new List<RefaccionTrabajoDto>()
                        };
                    }

                    return new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = "No se pudieron obtener las refacciones",
                        Refacciones = new List<RefaccionTrabajoDto>()
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error al obtener refacciones: {ex.Message}");
                    return new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = $"Error: {ex.Message}",
                        Refacciones = new List<RefaccionTrabajoDto>()
                    };
                }
            }

            /// <summary>
            /// Obtener todas las refacciones de una orden
            /// </summary>
        public async Task<ObtenerRefaccionesTrabajoResponse> ObtenerRefaccionesPorOrden(
                int ordenId)
            {
                try
                {
                    Debug.WriteLine($"📥 Obteniendo refacciones de la orden {ordenId}");

                    var response = await _httpClient.GetAsync(
                        $"{BaseUrl}/RefaccionesTrabajo/orden/{ordenId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content
                            .ReadFromJsonAsync<ObtenerRefaccionesTrabajoResponse>();

                        Debug.WriteLine($"✅ Se obtuvieron {result?.Refacciones.Count ?? 0} refacciones de la orden");

                        return result ?? new ObtenerRefaccionesTrabajoResponse
                        {
                            Success = false,
                            Message = "Error al procesar la respuesta",
                            Refacciones = new List<RefaccionTrabajoDto>()
                        };
                    }

                    return new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = "No se pudieron obtener las refacciones",
                        Refacciones = new List<RefaccionTrabajoDto>()
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error al obtener refacciones de orden: {ex.Message}");
                    return new ObtenerRefaccionesTrabajoResponse
                    {
                        Success = false,
                        Message = $"Error: {ex.Message}",
                        Refacciones = new List<RefaccionTrabajoDto>()
                    };
                }
            }

            /// <summary>
            /// Eliminar una refacción específica de un trabajo
            /// </summary>
        public async Task<AuthResponse> EliminarRefaccionTrabajo(int refaccionId)
            {
                try
                {
                    Debug.WriteLine($"🗑️ Eliminando refacción {refaccionId}");

                    var response = await _httpClient.DeleteAsync(
                        $"{BaseUrl}/RefaccionesTrabajo/{refaccionId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content
                            .ReadFromJsonAsync<AuthResponse>();

                        Debug.WriteLine($"✅ Refacción eliminada exitosamente");

                        return result ?? new AuthResponse
                        {
                            Success = false,
                            Message = "Error al procesar la respuesta"
                        };
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ Error HTTP {response.StatusCode}: {errorContent}");

                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Error al eliminar: {response.StatusCode}"
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error al eliminar refacción: {ex.Message}");
                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Error: {ex.Message}"
                    };
                }
            }

            /// <summary>
            /// Agregar una sola refacción a un trabajo (método simplificado)
            /// </summary>
        public async Task<AgregarRefaccionesResponse> AgregarRefaccionSimple(
                int trabajoId,
                string nombreRefaccion,
                int cantidad,
                decimal precioUnitario)
           {
                var request = new AgregarRefaccionesTrabajoRequest
                {
                    TrabajoId = trabajoId,
                    Refacciones = new List<AgregarRefaccionDto>
                {
                    new AgregarRefaccionDto
                    {
                        Refaccion = nombreRefaccion,
                        Cantidad = cantidad,
                        PrecioUnitario = precioUnitario
                    }
                }
            };

                return await AgregarRefaccionesTrabajo(request);
        }

            /// <summary>
            /// Validar si un trabajo tiene refacciones
            /// </summary>
        public async Task<bool> TrabajoTieneRefacciones(int trabajoId)
            {
                try
                {
                    var response = await ObtenerRefaccionesPorTrabajo(trabajoId);
                    return response.Success && response.TieneRefacciones;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Calcular total de refacciones de un trabajo
            /// </summary>
        public async Task<decimal> ObtenerTotalRefaccionesTrabajo(int trabajoId)
        {
                try
                {
                    var response = await ObtenerRefaccionesPorTrabajo(trabajoId);
                    return response.Success ? response.TotalRefacciones : 0;
                }
                catch
                {
                    return 0;
                }
            }
        }
}


