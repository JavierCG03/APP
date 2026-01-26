using CarslineApp.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        /// <summary>
        /// Guardar checklist de servicio
        /// </summary>
        public async Task<AuthResponse> GuardarCheckListAsync(CheckListServicioModel checkList)
        {
            try
            {
                // Serializar para debug
                var json = JsonSerializer.Serialize(checkList, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                System.Diagnostics.Debug.WriteLine("📤 JSON ENVIADO:");
                System.Diagnostics.Debug.WriteLine(json);

                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/CheckList/guardar",
                    checkList);

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📥 RESPUESTA HTTP {response.StatusCode}:");
                System.Diagnostics.Debug.WriteLine(responseContent);

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
                    Message = $"Error HTTP {response.StatusCode}: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPCIÓN en GuardarCheckListAsync: {ex.Message}");
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener checklist por ID de trabajo
        /// </summary>
        public async Task<CheckListServicioModel?> ObtenerCheckListPorTrabajoAsync(int trabajoId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/CheckList/trabajo/{trabajoId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CheckListServicioModel>();
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener checklist: {ex.Message}");
                return null;
            }
        }
    }
}