

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarslineApp.Models
{

       
        public class EvidenciaUpload
        {
            public string Descripcion { get; set; }
            public byte[] ImagenBytes { get; set; }
            public string NombreArchivo { get; set; }
        }

        public class EvidenciaDto
        {
            public int Id { get; set; }
            public int OrdenGeneralId { get; set; }
            public string RutaImagen { get; set; }
            public string Descripcion { get; set; }
            public DateTime? FechaRegistro { get; set; }
            public bool Activo { get; set; }
        }

        // Respuesta genérica del API
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }

    public class EvidenciaTrabajoItem : INotifyPropertyChanged
    {
        private byte[]? _imagenBytes;
        private string? _nombreArchivo;
        private bool _tieneImagen;
        private ImageSource? _imagenPreview;

        public string TipoEvidencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public byte[]? ImagenBytes
        {
            get => _imagenBytes;
            set
            {
                _imagenBytes = value;
                OnPropertyChanged();
            }
        }

        public string? NombreArchivo
        {
            get => _nombreArchivo;
            set
            {
                _nombreArchivo = value;
                OnPropertyChanged();
            }
        }

        public bool TieneImagen
        {
            get => _tieneImagen;
            set
            {
                _tieneImagen = value;
                OnPropertyChanged();
            }
        }

        public ImageSource? ImagenPreview
        {
            get => _imagenPreview;
            set
            {
                _imagenPreview = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class EvidenciaVisualizadorItem : INotifyPropertyChanged
    {
        private int _evidenciaId;
        private string _descripcion;
        private byte[] _imagenBytes;
        private bool _tieneImagen;
        private bool _estaCargando;
        private ImageSource _imagenPreview;
        private DateTime? _fechaRegistro;

        public int EvidenciaId
        {
            get => _evidenciaId;
            set
            {
                if (_evidenciaId != value)
                {
                    _evidenciaId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (_descripcion != value)
                {
                    _descripcion = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte[] ImagenBytes
        {
            get => _imagenBytes;
            set
            {
                if (_imagenBytes != value)
                {
                    _imagenBytes = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TieneImagen
        {
            get => _tieneImagen;
            set
            {
                if (_tieneImagen != value)
                {
                    _tieneImagen = value;
                    OnPropertyChanged();
                    System.Diagnostics.Debug.WriteLine($"🔔 TieneImagen cambió a {value} para evidencia {EvidenciaId}");
                }
            }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set
            {
                if (_estaCargando != value)
                {
                    _estaCargando = value;
                    OnPropertyChanged();
                    System.Diagnostics.Debug.WriteLine($"🔔 EstaCargando cambió a {value} para evidencia {EvidenciaId}");
                }
            }
        }

        public ImageSource ImagenPreview
        {
            get => _imagenPreview;
            set
            {
                if (_imagenPreview != value)
                {
                    _imagenPreview = value;
                    OnPropertyChanged();
                    System.Diagnostics.Debug.WriteLine($"🔔 ImagenPreview {(value != null ? "asignada" : "null")} para evidencia {EvidenciaId}");
                }
            }
        }

        public DateTime? FechaRegistro
        {
            get => _fechaRegistro;
            set
            {
                if (_fechaRegistro != value)
                {
                    _fechaRegistro = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FechaFormateada));
                }
            }
        }

        public string FechaFormateada
        {
            get
            {
                try
                {
                    return FechaRegistro?.ToString("dd/MMM/yyyy HH:mm") ?? "Sin fecha";
                }
                catch
                {
                    return "Fecha inválida";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
