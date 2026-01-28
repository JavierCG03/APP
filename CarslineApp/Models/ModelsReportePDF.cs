
namespace CarslineApp.Models
{
    /// <summary>
    /// Response para descargar PDF
    /// </summary>
    public class DescargarPdfResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[]? PdfBytes { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response para guardar PDF en servidor
    /// </summary>
    public class GuardarPdfResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RutaArchivo { get; set; }
        public string? NombreArchivo { get; set; }
        public string? NumeroOrden { get; set; }
    }

    /// <summary>
    /// Response para vista previa del PDF
    /// </summary>
    public class PdfPreviewResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PdfBase64 { get; set; }
        public string? NumeroOrden { get; set; }
        public int TamanoBytes { get; set; }
    }

}