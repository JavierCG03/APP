using CarslineApp.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace CarslineApp.Services
{
    public partial class ApiService
    {

        // Subir múltiples evidencias con imágenes
        public async Task<ApiResponse<List<EvidenciaDto>>> SubirEvidencias(
            int ordenGeneralId,
            List<EvidenciaUpload> evidencias)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                // Agregar OrdenGeneralId
                content.Add(new StringContent(ordenGeneralId.ToString()), "OrdenGeneralId");

                // Agregar cada imagen y descripción de forma secuencial
                int indice = 0;
                foreach (var evidencia in evidencias)
                {
                    if (evidencia.ImagenBytes != null && evidencia.ImagenBytes.Length > 0)
                    {
                        // Agregar imagen
                        var byteContent = new ByteArrayContent(evidencia.ImagenBytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                        string nombreArchivo = evidencia.NombreArchivo ?? $"imagen_{indice}.jpg";
                        content.Add(byteContent, "Imagenes", nombreArchivo);

                        // Agregar descripción - USAR SOLO 2 PARÁMETROS (sin fileName)
                        content.Add(new StringContent(evidencia.Descripcion ?? "Sin descripción"), "Descripciones");

                        indice++;
                    }
                }

                // Verificar que hay contenido para enviar
                if (indice == 0)
                {
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = "No hay imágenes válidas para subir"
                    };
                }

                // Construir URL completa: BaseUrl + endpoint específico
                string url = $"{BaseUrl}/EvidenciasOrden/Recepcion";
                System.Diagnostics.Debug.WriteLine($"URL de subida: {url}");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Intentar parsear la respuesta
                    try
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<dynamic>();
                        return new ApiResponse<List<EvidenciaDto>>
                        {
                            Success = true,
                            Message = "Evidencias guardadas correctamente",
                            Data = new List<EvidenciaDto>()
                        };
                    }
                    catch
                    {
                        // Si no se puede parsear, al menos fue exitoso
                        return new ApiResponse<List<EvidenciaDto>>
                        {
                            Success = true,
                            Message = "Evidencias guardadas correctamente"
                        };
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error del servidor: {error}");

                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = $"Error al subir evidencias: {response.StatusCode} - {error}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = "La solicitud tardó demasiado tiempo. Verifica tu conexión."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inesperado: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }
        public async Task<ApiResponse<List<EvidenciaDto>>> SubirEvidenciasTrabajo(
        int ordenGeneralId,
        List<EvidenciaUpload> evidencias)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                // Agregar OrdenGeneralId
                content.Add(new StringContent(ordenGeneralId.ToString()), "OrdenGeneralId");

                // Agregar cada imagen y descripción de forma secuencial
                int indice = 0;
                foreach (var evidencia in evidencias)
                {
                    if (evidencia.ImagenBytes != null && evidencia.ImagenBytes.Length > 0)
                    {
                        // Agregar imagen
                        var byteContent = new ByteArrayContent(evidencia.ImagenBytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                        string nombreArchivo = evidencia.NombreArchivo ?? $"imagen_{indice}.jpg";
                        content.Add(byteContent, "Imagenes", nombreArchivo);

                        // Agregar descripción - USAR SOLO 2 PARÁMETROS (sin fileName)
                        content.Add(new StringContent(evidencia.Descripcion ?? "Sin descripción"), "Descripciones");

                        indice++;
                    }
                }

                // Verificar que hay contenido para enviar
                if (indice == 0)
                {
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = "No hay imágenes válidas para subir"
                    };
                }

                // Construir URL completa: BaseUrl + endpoint específico
                string url = $"{BaseUrl}/EvidenciasOrden/Trabajo";
                System.Diagnostics.Debug.WriteLine($"URL de subida: {url}");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Intentar parsear la respuesta
                    try
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<dynamic>();
                        return new ApiResponse<List<EvidenciaDto>>
                        {
                            Success = true,
                            Message = "Evidencias guardadas correctamente",
                            Data = new List<EvidenciaDto>()
                        };
                    }
                    catch
                    {
                        // Si no se puede parsear, al menos fue exitoso
                        return new ApiResponse<List<EvidenciaDto>>
                        {
                            Success = true,
                            Message = "Evidencias guardadas correctamente"
                        };
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error del servidor: {error}");

                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = $"Error al subir evidencias: {response.StatusCode} - {error}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = "La solicitud tardó demasiado tiempo. Verifica tu conexión."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inesperado: {ex.Message}");
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }
        // Obtener evidencias por orden
        public async Task<ApiResponse<List<EvidenciaDto>>> ObtenerEvidenciasPorOrden(int ordenGeneralId)
        {
            try
            {
                // Construir URL completa
                string url = $"{BaseUrl}/EvidenciasOrden/Recepcion/{ordenGeneralId}";
                System.Diagnostics.Debug.WriteLine($"URL de consulta: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var evidencias = await response.Content.ReadFromJsonAsync<List<EvidenciaDto>>();
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = true,
                        Data = evidencias,
                        Message = "Evidencias obtenidas correctamente"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = true, // Cambiar a true porque no es un error
                        Data = new List<EvidenciaDto>(),
                        Message = "No se encontraron evidencias para esta orden"
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = $"Error al obtener evidencias: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Obtener evidencias por orden
        public async Task<ApiResponse<List<EvidenciaDto>>> ObtenerEvidenciasTrabajoPorOrden(int ordenGeneralId)
        {
            try
            {
                // Construir URL completa
                string url = $"{BaseUrl}/EvidenciasOrden/Trabajo/{ordenGeneralId}";
                System.Diagnostics.Debug.WriteLine($"URL de consulta: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var evidencias = await response.Content.ReadFromJsonAsync<List<EvidenciaDto>>();
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = true,
                        Data = evidencias,
                        Message = "Evidencias obtenidas correctamente"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = true, // Cambiar a true porque no es un error
                        Data = new List<EvidenciaDto>(),
                        Message = "No se encontraron evidencias para esta orden"
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<EvidenciaDto>>
                    {
                        Success = false,
                        Message = $"Error al obtener evidencias: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<EvidenciaDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Obtener imagen como bytes
        public async Task<ApiResponse<byte[]>> ObtenerImagen(int evidenciaId)
        {
            try
            {
                // Construir URL completa
                string url = $"{BaseUrl}/EvidenciasOrden/imagen/{evidenciaId}";
                System.Diagnostics.Debug.WriteLine($"URL de imagen: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return new ApiResponse<byte[]>
                    {
                        Success = true,
                        Data = imageBytes,
                        Message = "Imagen obtenida correctamente"
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = $"Error al obtener imagen: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Eliminar evidencia
        public async Task<ApiResponse<bool>> EliminarEvidencia(int evidenciaId)
        {
            try
            {
                // Construir URL completa
                string url = $"{BaseUrl}/EvidenciasOrden/{evidenciaId}";
                System.Diagnostics.Debug.WriteLine($"URL de eliminación: {url}");

                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Evidencia eliminada correctamente"
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Error al eliminar evidencia: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Obtener URL de imagen para visualización
        public string ObtenerUrlImagen(int evidenciaId)
        {
            return $"{BaseUrl}/EvidenciasOrden/imagen/{evidenciaId}";
        }
    }
}