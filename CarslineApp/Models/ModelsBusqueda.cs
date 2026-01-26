namespace CarslineApp.Models
{
    public enum TipoResultadoBusqueda
    {
        Cliente = 1,
        Vehiculo = 2,
        Orden = 3
    }

    public class ResultadoBusquedaDto
    {
        public int Id { get; set; }
        public TipoResultadoBusqueda Tipo { get; set; }
        public string TipoTexto { get; set; } = string.Empty;
        public string TituloPrincipal { get; set; } = string.Empty;
        public string Subtitulo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string IconoColor { get; set; } = "#2196F3";
        public string Icono { get; set; } = "👤";
    }

    public class BusquedaUnificadaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ResultadoBusquedaDto> Resultados { get; set; } = new();
        public int TotalResultados { get; set; }
        public string TerminoBusqueda { get; set; } = string.Empty;
    }

    public class OrdenSimpleDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string VehiculoInfo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int EstadoOrdenId { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
    }

    public class BuscarOrdenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrdenSimpleDto? Orden { get; set; }
    }
}