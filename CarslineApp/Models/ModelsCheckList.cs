
namespace CarslineApp.Models
{
    public class CheckListServicioModel
    {
        public int TrabajoId { get; set; }
        public int OrdenId { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public string? ComentariosTecnico { get; set; }

        //Sistema de Direccion
        public string Bieletas { get; set; } = string.Empty;
        public string Terminales { get; set; } = string.Empty;
        public string CajaDireccion { get; set; } = string.Empty;
        public string Volante { get; set; } = string.Empty;

        //Sistema de Suspension
        public string AmortiguadoresDelanteros { get; set; } = string.Empty;
        public string AmortiguadoresTraseros { get; set; } = string.Empty;
        public string BarraEstabilizadora { get; set; } = string.Empty;
        public string Horquillas { get; set; } = string.Empty;

        //Neumaticos
        public string NeumaticosDelanteros { get; set; } = string.Empty;
        public string NeumaticosTraseros { get; set; } = string.Empty;
        public string Balanceo { get; set; } = string.Empty;
        public string Alineacion { get; set; } = string.Empty;

        //Luces
        public string LucesAltas { get; set; } = string.Empty;
        public string LucesBajas { get; set; } = string.Empty;
        public string LucesAntiniebla { get; set; } = string.Empty;
        public string LucesReversa { get; set; } = string.Empty;
        public string LucesDireccionales { get; set; } = string.Empty;
        public string LucesIntermitentes { get; set; } = string.Empty;

        //Sistema de Frenos
        public string DiscosTamboresDelanteros { get; set; } = string.Empty;
        public string DiscosTamboresTraseros { get; set; } = string.Empty;
        public string BalatasDelanteras { get; set; } = string.Empty;
        public string BalatasTraseras { get; set; } = string.Empty;

        // Piezas del vehiculo Remplazadas
        public bool ReemplazoAceiteMotor { get; set; } = false;
        public bool ReemplazoFiltroAceite { get; set; } = false;
        public bool ReemplazoFiltroAireMotor { get; set; } = false;
        public bool ReemplazoFiltroAirePolen { get; set; } = false;

        // Revision de Niveles 
        public bool NivelLiquidoFrenos { get; set; } = false;
        public bool NivelAnticongelante { get; set; } = false;
        public bool NivelDepositoLimpiaparabrisas { get; set; } = false;
        public bool NivelAceiteMotor { get; set; } = false;
        //Trabajos realizados al vehiculo
        public bool DescristalizacionTamboresDiscos { get; set; } = false;
        public bool AjusteFrenos { get; set; } = false;
        public bool CalibracionPresionNeumaticos { get; set; } = false;
        public bool TorqueNeumaticos { get; set; } = false;
        public bool RotacionNeumaticos { get; set; } = false;

    }
}