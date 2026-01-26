
namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE CLIENTE (SIN CAMBIOS)
    // ============================================

    public class ClienteDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string TelefonoMovil { get; set; } = string.Empty;
        public string TelefonoCasa { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string NumeroExterior { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Pais { get; set; } = "México";
        public string CodigoPostal { get; set; } = string.Empty;
        public string InfoResumen => $"{NombreCompleto}\nTel: {TelefonoMovil} | RFC: {RFC}";
    }

    public class ClienteRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
        public string TelefonoMovil { get; set; } = string.Empty;
        public string TelefonoCasa { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string NumeroExterior { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Pais { get; set; } = "México";
        public string CodigoPostal { get; set; } = string.Empty;
    }

    public class ClienteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public ClienteDto Cliente { get; set; }
    }

    public class BuscarClientesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ClienteDto> Clientes { get; set; } = new();
    }
}
