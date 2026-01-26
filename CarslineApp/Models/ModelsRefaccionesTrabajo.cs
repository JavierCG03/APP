using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE REFACCIONES POR TRABAJO
    // ============================================

    /// <summary>
    /// DTO para una refacción individual del trabajo
    /// </summary>
    public class RefaccionTrabajoDto
    {
        public int Id { get; set; }
        public int TrabajoId { get; set; }
        public int OrdenGeneralId { get; set; }
        public string Refaccion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }

        // Propiedades calculadas para UI
        public string PrecioFormateado => $"${PrecioUnitario:N2}";
        public string TotalFormateado => $"${Total:N2}";
        public string CantidadTexto => $"{Cantidad}";
    }

    /// <summary>
    /// DTO para agregar una refacción al trabajo (con INotifyPropertyChanged para binding)
    /// </summary>
    public class AgregarRefaccionDto : INotifyPropertyChanged
    {
        private string _refaccion = string.Empty;
        private int _cantidad = 1;
        private decimal _precioUnitario = 0;

        public string Refaccion
        {
            get => _refaccion;
            set
            {
                _refaccion = value;
                OnPropertyChanged();
            }
        }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCalculado));
            }
        }

        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set
            {
                _precioUnitario = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCalculado));
            }
        }

        // Propiedades calculadas
        public decimal TotalCalculado => Cantidad * PrecioUnitario;
        public string TotalFormateado => $"${TotalCalculado:N2}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Request para agregar múltiples refacciones a un trabajo
    /// </summary>
    public class AgregarRefaccionesTrabajoRequest
    {
        public int TrabajoId { get; set; }
        public List<AgregarRefaccionDto> Refacciones { get; set; } = new();

        // Validación
        public bool EsValido =>
            TrabajoId > 0 &&
            Refacciones.Any() &&
            Refacciones.All(r =>
                !string.IsNullOrWhiteSpace(r.Refaccion) &&
                r.Cantidad > 0 &&
                r.PrecioUnitario > 0);
    }

    /// <summary>
    /// Respuesta al agregar refacciones
    /// </summary>
    public class AgregarRefaccionesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RefaccionTrabajoDto> RefaccionesAgregadas { get; set; } = new();
        public decimal TotalRefacciones { get; set; }
        public int CantidadRefacciones { get; set; }

        // Propiedades calculadas
        public string TotalFormateado => $"${TotalRefacciones:N2}";
        public bool TieneRefacciones => RefaccionesAgregadas.Any();
    }

    /// <summary>
    /// Respuesta para obtener refacciones de un trabajo
    /// </summary>
    public class ObtenerRefaccionesTrabajoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TrabajoId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public List<RefaccionTrabajoDto> Refacciones { get; set; } = new();
        public decimal TotalRefacciones { get; set; }

        // Propiedades calculadas
        public string TotalFormateado => $"${TotalRefacciones:N2}";
        public bool TieneRefacciones => Refacciones.Any();
        public int CantidadRefacciones => Refacciones.Count;
    }

    /// <summary>
    /// ViewModel para gestionar refacciones en una vista
    /// </summary>
    public class RefaccionTrabajoViewModel : INotifyPropertyChanged
    {
        private RefaccionTrabajoDto _refaccion;
        private bool _seleccionada;

        public RefaccionTrabajoViewModel(RefaccionTrabajoDto refaccion)
        {
            _refaccion = refaccion;
        }

        public RefaccionTrabajoDto Refaccion
        {
            get => _refaccion;
            set
            {
                _refaccion = value;
                OnPropertyChanged();
            }
        }

        public bool Seleccionada
        {
            get => _seleccionada;
            set
            {
                _seleccionada = value;
                OnPropertyChanged();
            }
        }

        // Propiedades de acceso rápido
        public int Id => Refaccion.Id;
        public string Nombre => Refaccion.Refaccion;
        public string CantidadTexto => Refaccion.CantidadTexto;
        public string PrecioFormateado => Refaccion.PrecioFormateado;
        public string TotalFormateado => Refaccion.TotalFormateado;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}