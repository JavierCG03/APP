
namespace CarslineApp.Models
{
    public class Cliente
        {
            public string Nombre { get; set; }
            public string Telefono { get; set; }
            public string Email { get; set; }
            public string Direccion { get; set; }
        }

        // Modelo para datos del vehículo
    public class Vehiculo
        {
            public string Marca { get; set; }
            public string Modelo { get; set; }
            public string Año { get; set; }
            public string Color { get; set; }
            public string Placas { get; set; }
            public string Kilometraje { get; set; }
        }

        // Modelo para trabajos realizados
    public class Trabajo
        {
            public string Descripcion { get; set; }
            public bool Completado { get; set; }

            public string Estado => Completado ? "✓ Completado" : "⏳ Pendiente";
        }

        // Modelo para refacciones
    public class Refaccion
        {
            public string Descripcion { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }

            public decimal Subtotal => Cantidad * PrecioUnitario;
        }

        // Modelo para mano de obra
    public class ManoObra
        {
            public string Descripcion { get; set; }
            public decimal Horas { get; set; }
            public decimal PrecioPorHora { get; set; }

            public decimal Subtotal => Horas * PrecioPorHora;
        }

        // Modelo principal de la orden
    public class OrdenTrabajo
        {
            public string NumeroOrden { get; set; }
            public DateTime Fecha { get; set; }
            public Cliente Cliente { get; set; }
            public Vehiculo Vehiculo { get; set; }
            public List<Trabajo> Trabajos { get; set; }
            public List<Refaccion> Refacciones { get; set; }
            public List<ManoObra> ManosObra { get; set; }
            public string Observaciones { get; set; }

            // Propiedades calculadas
            public decimal TotalRefacciones => Refacciones?.Sum(r => r.Subtotal) ?? 0;
            public decimal TotalManoObra => ManosObra?.Sum(m => m.Subtotal) ?? 0;
            public decimal Subtotal => TotalRefacciones + TotalManoObra;
            public decimal IVA => Subtotal * 0.16m;
            public decimal Total => Subtotal + IVA;

            public OrdenTrabajo()
            {
                Trabajos = new List<Trabajo>();
                Refacciones = new List<Refaccion>();
                ManosObra = new List<ManoObra>();
            }
        }

        // Información del taller
    public class InfoTaller
        {
            public string Nombre { get; set; }
            public string Direccion { get; set; }
            public string Telefono { get; set; }
            public string Email { get; set; }
        }

}
