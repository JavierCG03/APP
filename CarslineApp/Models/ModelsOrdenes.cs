using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CarslineApp.Models
{
    // ============================================
    // ✅ MODELOS DE ÓRDENES - ACTUALIZADOS
    // ============================================

    /// <summary>
    /// DTO simplificado para lista de órdenes (dashboard)
    /// </summary>
    public class OrdenDetalladaDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string HoraPromesa { get; set; } = string.Empty;
        public string FechaPromesa { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public int EstadoId { get; set; }
        public string TipoServicio { get; set; } = string.Empty;
        public bool TieneEvidencia { get; set; }


        // ✅ NUEVOS CAMPOS
        public int TotalTrabajos { get; set; }
        public int TrabajosCompletados { get; set; }
        public decimal ProgresoGeneral { get; set; }

        // Propiedades calculadas
        public bool NoTieneEvidencia => !TieneEvidencia;
        public bool EsServicio => TipoServicio != "Sin Servicio";
        public bool NoEsServicio => !EsServicio;
        public bool EsPendiente => EstadoId == 1;
        public bool EsProceso => EstadoId == 2;
        public bool EsFinalizada => EstadoId == 3;

        // ✅ NUEVAS PROPIEDADES CALCULADAS
        public string ProgresoTexto => $"{TrabajosCompletados}/{TotalTrabajos}";
        public string ProgresoFormateado => $"{ProgresoGeneral:F1}%";
        public bool TieneTrabajos => TotalTrabajos > 0;
        public double ProgressBar
        {
            get
            {
                if (TotalTrabajos == 0 || TrabajosCompletados == 0) return 0;
                return (double)TrabajosCompletados / (double)TotalTrabajos;
            }
        }

        // Color de progreso
        public Color ColorProgreso
        {
            get
            {
                if (ProgresoGeneral >= 100) return Color.FromArgb("#4CAF50"); // Verde
                if (ProgresoGeneral >= 50) return Color.FromArgb("#FF9800");  // Naranja
                return Color.FromArgb("#2196F3"); // Azul
            }
        }
    }
        /// <summary>
        /// DTO completo de orden con trabajos (vista detalle)
        /// </summary>
    public class OrdenConTrabajosDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int TipoOrdenId { get; set; }
        public string TipoOrden { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public string AsesorNombre { get; set; } = string.Empty;
        public int KilometrajeActual { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public DateTime? HoraFin { get; set; } 
        public int EstadoOrdenId { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public int TotalTrabajos { get; set; }
        public int TrabajosCompletados { get; set; }
        public decimal ProgresoGeneral { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public bool TieneEvidencia { get; set; }

        // Lista de trabajos
        public List<TrabajoDto> Trabajos { get; set; } = new();

        // Propiedades calculadas
        public string Ultimos4VIN =>
        VIN.Length >= 4 ? VIN.Substring(VIN.Length - 4) : VIN;

        public string ProgresoTexto => $"{TrabajosCompletados}/{TotalTrabajos}";
        public string ProgresoFormateado => $"{ProgresoGeneral:F1}%";
        public bool TieneTrabajosEnProceso => Trabajos.Any(t => t.EnProceso);
        public bool TieneTrabajosCompletados => Trabajos.Any(t => t.EstaCompletado);
        public bool TieneTrabajos => Trabajos.Any();
        public string BackgroundOrden { get; set; }
        public string BorderOrden { get; set; }
        public string TextOrden { get; set; }
        public string BadgeColor { get; set; }
        public string ProgresoBackground { get; set; }
        public string ProgresoColor { get; set; }
        public string TrabajosHeaderColor { get; set; }
        public bool MostrarProgreso { get; set; }
        public bool MostrarBarraProgreso { get; set; }
        public double ProgressBar
        {
            get
            {
                if (TotalTrabajos == 0 || TrabajosCompletados == 0) return 0;
                return (double)TrabajosCompletados / (double)TotalTrabajos;
            }
        }
    }

    /// <summary>
    /// Request para crear orden con trabajos
    /// </summary>
    public class CrearOrdenConTrabajosRequest
    {
        public int TipoOrdenId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int TipoServicioId { get; set; }
        public int KilometrajeActual { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public List<TrabajoCrearDto> Trabajos { get; set; } = new();
    }

    /// <summary>
    /// ✅ AGREGADO: Response para crear orden con trabajos
    /// </summary>
    public class CrearOrdenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public int OrdenId { get; set; }
        public int TotalTrabajos { get; set; }
        public decimal CostoTotal { get; set; }
    }

    // ============================================
    // MODELOS DE SERVICIOS (REFERENCIA)
    // ============================================

    public class TipoServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";
    }
    public class ServicioExtraDto : INotifyPropertyChanged
    {
        private bool _seleccionado;
        private string _indicacionesPersonalizadas = string.Empty;

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";

        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                _seleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarIndicaciones));
            }
        }

        // Nueva propiedad para indicaciones personalizadas del asesor
        public string IndicacionesPersonalizadas
        {
            get => _indicacionesPersonalizadas;
            set
            {
                _indicacionesPersonalizadas = value;
                OnPropertyChanged();
            }
        }

        // Propiedad para controlar la visibilidad del campo de indicaciones
        public bool MostrarIndicaciones => Seleccionado;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TrabajoDto
    {
        public int Id { get; set; }
        public int OrdenGeneralId { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public int? TecnicoAsignadoId { get; set; }
        public string? TecnicoNombre { get; set; }
        public DateTime? FechaHoraAsignacionTecnico { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraTermino { get; set; }
        public string? DuracionFormateada { get; set; }
        public string? IndicacionesTrabajo { get; set; }
        public string? ComentariosTecnico { get; set; }
        public string? ComentariosJefeTaller { get; set; }
        public int EstadoTrabajo { get; set; }
        public string? EstadoTrabajoNombre { get; set; }
        public string? ColorEstado { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas de estado
        public bool EsPendiente => EstadoTrabajo == 1;
        public bool EnProceso => EstadoTrabajo == 3;
        public bool EstaCompletado => EstadoTrabajo == 4;
        public bool EstaPausado => EstadoTrabajo == 5;
        public bool EstaCancelado => EstadoTrabajo == 6;

        // ✅ NUEVAS PROPIEDADES para asignación de técnicos
        public bool TieneIndicaciones => !string.IsNullOrWhiteSpace(IndicacionesTrabajo);
        public bool TieneTecnicoAsignado => TecnicoAsignadoId.HasValue && TecnicoAsignadoId > 0;
        public bool NoTieneTecnicoAsignado => !TieneTecnicoAsignado;

        // Solo se puede reasignar si tiene técnico Y no está en proceso
        public bool PuedeReasignar => TieneTecnicoAsignado && !EnProceso && !EstaCompletado && !EstaCancelado;

        // Solo se puede asignar si NO tiene técnico Y está pendiente
        public bool PuedeAsignar => NoTieneTecnicoAsignado;

        // Texto para mostrar estado de técnico
        public string TextoTecnico => TieneTecnicoAsignado
            ? $"👨‍🔧 {TecnicoNombre}"
            : "Sin técnico asignado";

        // Color visual según estado
        public Color ColorVisualEstado => EstadoTrabajo switch
        {
            1 => Color.FromArgb("#E53935"), // Pendiente - Rojo
            2 => Color.FromArgb("#E53935"), // Pendiente - Rojo
            3 => Color.FromArgb("#FDD835"), // En Proceso - Amarillo
            4 => Color.FromArgb("#43A047"), // Completado - Verde
            5 => Color.FromArgb("#FB8C00"), // Pausado - Naranja oscuro
            6 => Color.FromArgb("#1A1A1A"), // Cancelado - Negro suavizado
            _ => Colors.Gray // Default
        };
        public bool MostrarEstado { get; set; }
        public bool NoMostrarEstado { get; set; }
        public bool TieneTecnico { get; set; }
    }
    public class TrabajoCrearDto
    {
        [Required]
        public string Trabajo { get; set; } = string.Empty;

        public string? Indicaciones { get; set; }
    }

    public class MisTrabajosResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TecnicoId { get; set; }
        public string TecnicoNombre { get; set; } = string.Empty;
        public int? FiltroEstado { get; set; }
        public int TotalTrabajos { get; set; }
        public List<MiTrabajoDto> Trabajos { get; set; } = new();
    }
    // DTO específico para "Mis Trabajos"

    public class MiTrabajoDto
    {
        public int Id { get; set; }
        public int OrdenGeneralId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int TipoOrden { get; set; }
        public string Trabajo { get; set; } = string.Empty;

        // Vehículo
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;

        // Trabajo
        public DateTime? FechaHoraAsignacionTecnico { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraTermino { get; set; }
        public string? IndicacionesTrabajo { get; set; }
        public string? ComentariosTecnico { get; set; }

        // Estado
        public int EstadoTrabajo { get; set; }
        public string? EstadoTrabajoNombre { get; set; }

        // Fechas
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaPromesaEntrega { get; set; }

        // Propiedades calculadas
        public bool EsPendiente => EstadoTrabajo == 1;
        public bool EstaAsignado => EstadoTrabajo == 2;
        public bool EnProceso => EstadoTrabajo == 3;
        public bool EstaCompletado => EstadoTrabajo == 4;
        public bool EstaPausado => EstadoTrabajo == 5;
        public bool EstaCancelado => EstadoTrabajo == 6;
        public string FechaFormateada => FechaPromesaEntrega.ToString("dd/MMM");
        public string HoraFormateada => FechaPromesaEntrega.ToString("hh:mm tt");
       
        public bool TieneIndicaciones => !string.IsNullOrWhiteSpace(IndicacionesTrabajo);
        public bool PuedeIniciar => EstadoTrabajo == 2; 

        public string DuracionFormateada
        {
            get
            {
                if (!FechaHoraInicio.HasValue || !FechaHoraTermino.HasValue)
                    return "-";

                var duracion = FechaHoraTermino.Value - FechaHoraInicio.Value;
                return $"{duracion.Hours}h {duracion.Minutes}m";
            }
        }

        public string TiempoTranscurrido
        {
            get
            {
                if (!FechaHoraInicio.HasValue)
                    return "-";

                var fechaFin = FechaHoraTermino ?? DateTime.Now;
                var duracion = fechaFin - FechaHoraInicio.Value;

                if (duracion.TotalHours >= 1)
                    return $"{(int)duracion.TotalHours}h {duracion.Minutes}m";
                else
                    return $"{duracion.Minutes}m";
            }
        }
        public string BorderColor => TipoOrden switch
        {
            1 => "#D60000",  // Servicio
            2 => "#2196F3",  // Diagnóstico
            3 => "#FFA000",  // Reparación
            4 => "#4CAF50",  // Garantía
            5 => "#9C27B0",  // Reacondicionamiento
            _ => "#888888"
        };
        public string BacColor => TipoOrden switch
        {
            1 => "#FFF5F5",  // Servicio
            2 => "#E3F2FD",  // Diagnóstico
            3 => "#FFF8E1",  // Reparación
            4 => "#E8F5E9",  // Garantía
            5 => "#F3E5F5",  // Reacondicionamiento
            _ => "#888888"
        };
        // Determinar si se muestra fecha completa o solo hora
        public bool MostrarFecha => TipoOrden != 1;
        public bool MostrarSoloHora => TipoOrden == 1;

        // Fecha y hora combinada
        public string FechaHoraCompleta => $"{FechaFormateada} {HoraFormateada}";
    }

    public class TrabajoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TrabajoSimpleDto
    {
        public string Trabajo { get; set; } = string.Empty;
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string TecnicoNombre { get; set; } = string.Empty;
        public string EstadoTrabajoNombre { get; set; } = string.Empty;
        public DateTime? FechaHoraAsignacionTecnico { get; set; }
    }
    public class TrabajoTableroItem
    {
        public string Trabajo { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaEntrega { get; set; }

        public string FechaEntregaFormateada => FechaEntrega?.ToString("dd/MMM.-hh:mm tt") ?? "Sin fecha";

        public string Icono => Estado switch
        {
            "Pendiente" => "⏳",
            "En Proceso" => "🔧",
            "Pausado" => "⏸️",
            "Completado" => "✅",
            _ => "📋"
        };
    }

    public class TecnicoTableroDetalle : INotifyPropertyChanged
    {
        private string _tecnicoNombre;
        private bool _isExpanded;

        public string TecnicoNombre
        {
            get => _tecnicoNombre;
            set
            {
                _tecnicoNombre = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TrabajoTableroItem> Trabajos { get; set; } = new();

        public int TotalTrabajos => Trabajos?.Count ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class FijarManoObraRequest
    {
        [Required(ErrorMessage = "El costo de mano de obra es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0")]
        public decimal CostoManoObra { get; set; }
    }

    public class ManoObraResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal CostoManoObra { get; set; }

    }
    public class TrabajoSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Trabajo { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;

    }
}