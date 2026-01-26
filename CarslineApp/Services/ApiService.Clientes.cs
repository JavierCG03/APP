using CarslineApp.Models;
using System.Net.Http.Json;


namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<BuscarClientesResponse> BuscarClientesPorNombreAsync(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 3)
                {
                    return new BuscarClientesResponse
                    {
                        Success = false,
                        Message = "Ingresa al menos 3 caracteres",
                        Clientes = new List<ClienteDto>()
                    };
                }

                var response = await _httpClient.GetAsync($"{BaseUrl}/Clientes/buscar-nombre/{Uri.EscapeDataString(nombre)}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BuscarClientesResponse>();
                    return result ?? new BuscarClientesResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta",
                        Clientes = new List<ClienteDto>()
                    };
                }

                return new BuscarClientesResponse
                {
                    Success = false,
                    Message = "Error en la búsqueda",
                    Clientes = new List<ClienteDto>()
                };
            }
            catch (Exception ex)
            {
                return new BuscarClientesResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Clientes = new List<ClienteDto>()
                };
            }
        }

        public async Task<ClienteResponse> ObtenerClientePorIdAsync(int clienteId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Clientes/{clienteId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ClienteResponse>();
                    return result ?? new ClienteResponse
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    };
                }

                return new ClienteResponse
                {
                    Success = false,
                    Message = "Cliente no encontrado"
                };
            }
            catch (Exception ex)
            {
                return new ClienteResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ClienteResponse> BuscarClientePorTelefonoAsync(string telefono)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Clientes/buscar-telefono/{telefono}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ClienteResponse>();
                    return result ?? new ClienteResponse
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    };
                }

                return new ClienteResponse
                {
                    Success = false,
                    Message = "Cliente no encontrado"
                };
            }
            catch (Exception ex)
            {
                return new ClienteResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ClienteResponse> CrearClienteAsync(ClienteRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Clientes/crear", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ClienteResponse>();
                    return result ?? new ClienteResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new ClienteResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new ClienteResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ClienteResponse> ActualizarClienteAsync(int clienteId, ClienteRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/Clientes/actualizar/{clienteId}",
                    request
                );

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<ClienteResponse>();
                    return resultado ?? new ClienteResponse
                    {
                        Success = false,
                        Message = "Error al deserializar respuesta"
                    };
                }

                return new ClienteResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new ClienteResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

    }
}
