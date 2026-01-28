
namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE REFACCIONES (SIN CAMBIOS)
    // ============================================

    public class RefaccionDto
    {
        public int Id { get; set; }
        public string NumeroParte { get; set; } = string.Empty;
        public string TipoRefaccion { get; set; } = string.Empty;
        public string? MarcaVehiculo { get; set; }
        public string? Ubicacion { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }

        public string DescripcionCompleta => $"{TipoRefaccion}" +
            (string.IsNullOrEmpty(MarcaVehiculo) ? "" : $" - {MarcaVehiculo}") +
            (string.IsNullOrEmpty(Modelo) ? "" : $" {Modelo}") +
            (Anio.HasValue ? $" {Anio}" : "");

        // ✅ Propiedades para la UI
        public string ColorTipo
        {
            get
            {
                return TipoRefaccion.ToLower() switch
                {
                    var t when t.Contains("filtro") => "#3498DB",
                    var t when t.Contains("balata") => "#E74C3C",
                    var t when t.Contains("bujia") => "#F39C12",
                    var t when t.Contains("amortiguador") => "#9B59B6",
                    _ => "#95A5A6"
                };
            }
        }

        public string IconoTipo
        {
            get
            {
                return TipoRefaccion.ToLower() switch
                {
                    var t when t.Contains("filtro") => "🔧",
                    var t when t.Contains("bujia") => "⚡",
                    var t when t.Contains("amortiguador") => "🔩",
                    _ => "📦"
                };
            }
        }

        public string ColorStock => Cantidad == 0 ? "#E74C3C" : Cantidad < 5 ? "#F39C12" : "#27AE60";
        public string ColorStockFondo => Cantidad == 0 ? "#FFEBEE" : Cantidad < 5 ? "#FFF3E0" : "#E8F5E9";
    }

    public class CrearRefaccionRequest
    {
        public string NumeroParte { get; set; } = string.Empty;
        public string TipoRefaccion { get; set; } = string.Empty;
        public string? MarcaVehiculo { get; set; }
        public string? Ubicacion { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public int Cantidad { get; set; }
    }

    public class RefaccionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RefaccionDto? Refaccion { get; set; }
    }
    // ✅ NUEVO DTO para respuesta paginada

    public class RefaccionesPaginadasResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty; // ✅ AGREGAR ESTA LÍNEA
        public List<RefaccionDto> Refacciones { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public int PorPagina { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }
}
