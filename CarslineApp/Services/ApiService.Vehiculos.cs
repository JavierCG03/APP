using CarslineApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {

        public async Task<BuscarVehiculosResponse> BuscarVehiculosPorUltimos4VINAsync(string ultimos4)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultimos4) || ultimos4.Length != 4)
                {
                    return new BuscarVehiculosResponse
                    {
                        Success = false,
                        Message = "Debes ingresar exactamente 4 caracteres",
                        Vehiculos = new List<VehiculoDto>()
                    };
                }

                var response = await _httpClient.GetAsync($"{BaseUrl}/Vehiculos/buscar-vin-ultimos/{ultimos4.ToUpper()}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BuscarVehiculosResponse>();
                    return result ?? new BuscarVehiculosResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta",
                        Vehiculos = new List<VehiculoDto>()
                    };
                }

                return new BuscarVehiculosResponse
                {
                    Success = false,
                    Message = "Error en la búsqueda",
                    Vehiculos = new List<VehiculoDto>()
                };
            }
            catch (Exception ex)
            {
                return new BuscarVehiculosResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Vehiculos = new List<VehiculoDto>()
                };
            }
        }

        public async Task<BuscarVehiculosResponse> BuscarVehiculosPorClienteIdAsync(int ClienteId)
        {
            try
            {
                if (ClienteId < 0) 
                {
                    return new BuscarVehiculosResponse
                    {
                        Success = false,
                        Message = "ID invalido",
                        Vehiculos = new List<VehiculoDto>()
                    };
                }

                var response = await _httpClient.GetAsync($"{BaseUrl}/Vehiculos/buscarClienteId/{ClienteId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BuscarVehiculosResponse>();
                    return result ?? new BuscarVehiculosResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta",
                        Vehiculos = new List<VehiculoDto>()
                    };
                }

                return new BuscarVehiculosResponse
                {
                    Success = false,
                    Message = "Error en la búsqueda",
                    Vehiculos = new List<VehiculoDto>()
                };
            }
            catch (Exception ex)
            {
                return new BuscarVehiculosResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Vehiculos = new List<VehiculoDto>()
                };
            }
        }

        public async Task<VehiculoResponse> ObtenerVehiculoPorIdAsync(int vehiculoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Vehiculos/{vehiculoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VehiculoResponse>();
                    return result ?? new VehiculoResponse
                    {
                        Success = false,
                        Message = "Vehículo no encontrado"
                    };
                }

                return new VehiculoResponse
                {
                    Success = false,
                    Message = "Vehículo no encontrado"
                };
            }
            catch (Exception ex)
            {
                return new VehiculoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<VehiculoResponse> BuscarVehiculoPorVINAsync(string vin)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Vehiculos/buscar-vin/{vin}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VehiculoResponse>();
                    return result ?? new VehiculoResponse
                    {
                        Success = false,
                        Message = "Vehículo no encontrado"
                    };
                }

                return new VehiculoResponse
                {
                    Success = false,
                    Message = "Vehículo no encontrado"
                };
            }
            catch (Exception ex)
            {
                return new VehiculoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<VehiculoResponse> CrearVehiculoAsync(VehiculoRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/Vehiculos/crear", request);

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<VehiculoResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new VehiculoResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return result ?? new VehiculoResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new VehiculoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<VehiculoResponse> ActualizarPlacasVehiculoAsync(int vehiculoId, string nuevasPlacas)
        {
            try
            {
                var request = new VehiculoRequest { Placas = nuevasPlacas };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/Vehiculos/actualizar-placas/{vehiculoId}",
                    request
                );

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<VehiculoResponse>();
                    return resultado ?? new VehiculoResponse
                    {
                        Success = false,
                        Message = "Error al deserializar respuesta"
                    };
                }

                return new VehiculoResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new VehiculoResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

    }
}
