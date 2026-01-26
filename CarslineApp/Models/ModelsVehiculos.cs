
namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE VEHÍCULO (SIN CAMBIOS)
    // ============================================

    public class VehiculoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public int KilometrajeInicial { get; set; }
        public string NombreCliente { get; set; } = string.Empty;

        public string InfoResumen =>
            $"{Marca} {Modelo} {Anio}\nVIN: ...{Ultimos4VIN} | Cliente: {NombreCliente}";

        public string VehiculoCompleto => $"{Marca} {Modelo} {Anio} - {Color}";
        public string Ultimos4VIN => VIN.Length >= 4 ? VIN.Substring(VIN.Length - 4) : VIN;
    }

    public class VehiculoRequest
    {
        public int ClienteId { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public int KilometrajeInicial { get; set; }
    }

    public class VehiculoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public VehiculoDto Vehiculo { get; set; }
    }

    public class BuscarVehiculosResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<VehiculoDto> Vehiculos { get; set; } = new();
    }
}
