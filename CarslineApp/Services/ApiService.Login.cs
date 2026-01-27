using CarslineApp.Models;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class  ApiService
    {
        public async Task<AuthResponse> LoginAsync(string nombreUsuario, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    NombreUsuario = nombreUsuario,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Auth/login", request);

                // Intentar leer siempre el contenido como AuthResponse
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new AuthResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                // Si falla, devolver el mensaje que vino del API
                return result ?? new AuthResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (HttpRequestException ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
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

        public async Task<CrearUsuarioResponse> CrearUsuarioAsync(
            string nombreCompleto,
            string nombreUsuario,
            string password,
            int rolId,
            int adminId)
        {
            try
            {
                var request = new CrearUsuarioRequest
                {
                    NombreCompleto = nombreCompleto,
                    NombreUsuario = nombreUsuario,
                    Password = password,
                    RolId = rolId
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/Auth/crear-usuario")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add("X-Admin-Id", adminId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CrearUsuarioResponse>();
                    return result ?? new CrearUsuarioResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new CrearUsuarioResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new CrearUsuarioResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<List<RolDto>> ObtenerRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Auth/roles");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<RolDto>>();
                    return result ?? new List<RolDto>();
                }
                return new List<RolDto>();
            }
            catch
            {
                return new List<RolDto>();
            }
        }

        public async Task<List<UsuarioDto>> ObtenerUsuariosAsync(int adminId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/Auth/usuarios");
                httpRequest.Headers.Add("X-Admin-Id", adminId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();
                    return result ?? new List<UsuarioDto>();
                }
                return new List<UsuarioDto>();
            }
            catch
            {
                return new List<UsuarioDto>();
            }
        }

        public async Task<List<UsuarioDto>> ObtenerTecnicosAsync()
        {
            try
            {
                // Obtener usuarios con rol de técnico (RolId = 5)
                var response = await _httpClient.GetAsync($"{BaseUrl}/Auth/Tecnicos");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();
                    return result ?? new List<UsuarioDto>();
                }
                return new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener técnicos: {ex.Message}");
                return new List<UsuarioDto>();
            }
        }

    }

}
