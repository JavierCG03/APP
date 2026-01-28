using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using CarslineApp.Views.Buscador;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelBuscador
{
    public class OrdenDetalleViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly PdfGeneratorService _pdfGenerator;
        private readonly int _ordenId;
        private OrdenConTrabajosDto _orden;
        private ClienteDto _cliente;
        private VehiculoDto _vehiculo;
        private bool _isLoading;
        private string _errorMessage;

        public OrdenDetalleViewModel(int ordenId)
        {
            _apiService = new ApiService();
            _pdfGenerator = new PdfGeneratorService();
            _ordenId = ordenId;

            // Comandos
            VerClienteCommand = new Command(async () => await VerCliente());
            VerVehiculoCommand = new Command(async () => await VerVehiculo());
            VerRefaccionesCommand = new Command<int>(async (id) => await VerRefaccionesTrabajo(id));
            VerEvidenciasCommand = new Command(async () => await VerEvidencias());
            CancelarOrdenCommand = new Command(async () => await CancelarOrden());
            EntregarOrdenCommand = new Command(async () => await EntregarOrden());
            RefreshCommand = new Command(async () => await CargarDatosOrden());
            VerEvidenciasTrabajoCommand = new Command(async () => await VerEvidenciasTrabajo());
            GenerarPdfCommand = new Command(async () => await GenerarPdf(), () => TieneOrden);

            _ = CargarDatosOrden();
        }

        #region Propiedades
        // Constructor con inyección de dependencias

        /*
        public OrdenConTrabajosDto Orden
        {
            get => _orden;
            set
            {
                _orden = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneOrden));
                OnPropertyChanged(nameof(EsPendiente));
                OnPropertyChanged(nameof(EsEnProceso));
                OnPropertyChanged(nameof(EsFinalizada));
                OnPropertyChanged(nameof(PuedeEntregar));
                OnPropertyChanged(nameof(PuedeCancelar));
                OnPropertyChanged(nameof(ColorEstado));
                OnPropertyChanged(nameof(IconoEstado));
            }
        }
        */
        public OrdenConTrabajosDto Orden
        {
            get => _orden;
            set
            {
                System.Diagnostics.Debug.WriteLine($"📝 SET Orden - Antes: {(_orden == null ? "NULL" : _orden.NumeroOrden)}");
                System.Diagnostics.Debug.WriteLine($"📝 SET Orden - Nuevo: {(value == null ? "NULL" : value.NumeroOrden)}");

                _orden = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneOrden));
                OnPropertyChanged(nameof(EsPendiente));
                OnPropertyChanged(nameof(EsEnProceso));
                OnPropertyChanged(nameof(EsFinalizada));
                OnPropertyChanged(nameof(PuedeEntregar));
                OnPropertyChanged(nameof(PuedeCancelar));
                OnPropertyChanged(nameof(ColorEstado));
                OnPropertyChanged(nameof(IconoEstado));

                // Actualizar comando
                try
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Actualizando CanExecute de GenerarPdfCommand...");
                    ((Command)GenerarPdfCommand).ChangeCanExecute();
                    System.Diagnostics.Debug.WriteLine($"✅ CanExecute actualizado - TieneOrden: {TieneOrden}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error actualizando CanExecute: {ex.Message}");
                }
            }
        }
        public ClienteDto Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        public VehiculoDto Vehiculo
        {
            get => _vehiculo;
            set { _vehiculo = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Propiedades calculadas
        public bool TieneOrden => Orden != null;
        public bool EsPendiente => Orden?.EstadoOrdenId == 1;
        public bool EsEnProceso => Orden?.EstadoOrdenId == 2;
        public bool EsFinalizada => Orden?.EstadoOrdenId == 3;
        public bool PuedeEntregar => EsFinalizada && Orden?.ProgresoGeneral >= 100;
        public bool PuedeCancelar => EsPendiente ;

        public string ColorEstado => Orden?.EstadoOrdenId switch
        {
            1 => "#FFA500", // Pendiente - Naranja
            2 => "#2196F3", // En Proceso - Azul
            3 => "#00BCD4", // Finalizada - Turquesa
            4 => "#4CAF50", // Entregada - Verde 
            5 => "#757575", // Cancelada - Gris oscuro
            _ => "#757575"  // Desconocido - Gris
        };

        public string IconoEstado => Orden?.EstadoOrdenId switch
        {
            1 => "📋",  // Pendiente
            2 => "⚙️",  // En Proceso
            3 => "✔️",  // Finalizada
            4 => "✅",  // Entregada
            5 =>  "❌",  // Cancelada          
            _ => "❓"   // Desconocido
        };
        #endregion

        #region Comandos

        public ICommand VerClienteCommand { get; }
        public ICommand VerVehiculoCommand { get; }
        public ICommand VerRefaccionesCommand { get; }
        public ICommand VerEvidenciasCommand { get; }
        public ICommand CancelarOrdenCommand { get; }
        public ICommand EntregarOrdenCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand VerEvidenciasTrabajoCommand { get; }
        public ICommand GenerarPdfCommand { get; }

        #endregion

        #region MetodosPDF
        private async Task GenerarPdf()
        {
            System.Diagnostics.Debug.WriteLine("🚀 ========== INICIO GenerarPdf ==========");

            try
            {
                System.Diagnostics.Debug.WriteLine($"📊 Estado inicial - Orden: {(Orden == null ? "NULL" : "OK")}");

                if (Orden == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ PASO 1 FALLÓ: Orden es NULL");
                    await MostrarAlerta("⚠️ Error", "No hay datos de la orden para generar el PDF");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ PASO 1: Orden existe");
                System.Diagnostics.Debug.WriteLine($"   - NumeroOrden: {Orden.NumeroOrden}");
                System.Diagnostics.Debug.WriteLine($"   - ClienteId: {Orden.ClienteId}");

                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("✅ PASO 2: IsLoading = true");

                ErrorMessage = string.Empty;
                System.Diagnostics.Debug.WriteLine("✅ PASO 3: ErrorMessage limpiado");

                // PASO 4: Crear datos para PDF
                System.Diagnostics.Debug.WriteLine("🔄 PASO 4: Iniciando creación de datos...");

                OrdenTrabajo ordenPdf = null;
                try
                {
                    ordenPdf = CrearOrdenEjemplo();
                    System.Diagnostics.Debug.WriteLine("✅ PASO 4A: CrearOrdenEjemplo() exitoso");
                    System.Diagnostics.Debug.WriteLine($"   - Trabajos: {ordenPdf.Trabajos?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"   - Refacciones: {ordenPdf.Refacciones?.Count ?? 0}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 4A FALLÓ: {ex.Message}");
                    throw;
                }

                InfoTaller tallerInfo = null;
                try
                {
                    tallerInfo = CrearInfoTaller();
                    System.Diagnostics.Debug.WriteLine("✅ PASO 4B: CrearInfoTaller() exitoso");
                    System.Diagnostics.Debug.WriteLine($"   - Nombre: {tallerInfo.Nombre}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 4B FALLÓ: {ex.Message}");
                    throw;
                }

                // PASO 5: Verificar servicio PDF
                System.Diagnostics.Debug.WriteLine("🔄 PASO 5: Verificando servicio PDF...");
                if (_pdfGenerator == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ PASO 5 FALLÓ: _pdfGenerator es NULL");
                    await MostrarAlerta("❌ Error", "El servicio de PDF no está inicializado");
                    return;
                }
                System.Diagnostics.Debug.WriteLine("✅ PASO 5: _pdfGenerator existe");

                // PASO 6: Generar el PDF
                System.Diagnostics.Debug.WriteLine("🔄 PASO 6: Llamando a GenerarOrdenTrabajoPDF...");
                string filePath = null;

                try
                {
                    filePath = await _pdfGenerator.GenerarOrdenTrabajoPDF(ordenPdf, tallerInfo);
                    System.Diagnostics.Debug.WriteLine($"✅ PASO 6: PDF generado exitosamente");
                    System.Diagnostics.Debug.WriteLine($"   - Ruta: {filePath}");
                    System.Diagnostics.Debug.WriteLine($"   - Existe archivo: {File.Exists(filePath)}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 6 FALLÓ: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"   - StackTrace: {ex.StackTrace}");
                    throw;
                }

                // PASO 7: Verificar que el archivo existe
                System.Diagnostics.Debug.WriteLine("🔄 PASO 7: Verificando archivo generado...");
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 7 FALLÓ: Archivo no existe o ruta vacía");
                    System.Diagnostics.Debug.WriteLine($"   - filePath: {filePath ?? "NULL"}");
                    await MostrarAlerta("❌ Error", "El PDF no se generó correctamente");
                    return;
                }
                System.Diagnostics.Debug.WriteLine("✅ PASO 7: Archivo verificado");

                // PASO 8: Mostrar mensaje de éxito
                System.Diagnostics.Debug.WriteLine("🔄 PASO 8: Mostrando alerta de éxito...");
                try
                {
                    await MostrarAlerta(
                        "✅ PDF Generado",
                        $"El PDF se generó correctamente:\n{Path.GetFileName(filePath)}");
                    System.Diagnostics.Debug.WriteLine("✅ PASO 8: Alerta mostrada");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 8 FALLÓ: {ex.Message}");
                    throw;
                }

                // PASO 9: Preguntar si quiere abrir
                System.Diagnostics.Debug.WriteLine("🔄 PASO 9: Preguntando si desea abrir...");
                bool abrir = false;
                try
                {
                    abrir = await Application.Current.MainPage.DisplayAlert(
                        "📄 Abrir PDF",
                        "¿Quieres abrir el PDF generado?",
                        "Sí",
                        "No");
                    System.Diagnostics.Debug.WriteLine($"✅ PASO 9: Respuesta = {abrir}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 9 FALLÓ: {ex.Message}");
                }

                if (abrir)
                {
                    System.Diagnostics.Debug.WriteLine("🔄 PASO 10: Intentando abrir PDF...");
                    try
                    {
                        await AbrirPdf(filePath);
                        System.Diagnostics.Debug.WriteLine("✅ PASO 10: PDF abierto");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ PASO 10 FALLÓ: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⏭️ PASO 10: Usuario decidió no abrir");
                }

                // PASO 11: Preguntar si quiere compartir
                System.Diagnostics.Debug.WriteLine("🔄 PASO 11: Preguntando si desea compartir...");
                bool compartir = false;
                try
                {
                    compartir = await Application.Current.MainPage.DisplayAlert(
                        "📤 Compartir",
                        "¿Deseas compartir el PDF?",
                        "Sí",
                        "No");
                    System.Diagnostics.Debug.WriteLine($"✅ PASO 11: Respuesta = {compartir}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PASO 11 FALLÓ: {ex.Message}");
                }

                if (compartir)
                {
                    System.Diagnostics.Debug.WriteLine("🔄 PASO 12: Intentando compartir PDF...");
                    try
                    {
                        await CompartirPdf(filePath);
                        System.Diagnostics.Debug.WriteLine("✅ PASO 12: PDF compartido");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ PASO 12 FALLÓ: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⏭️ PASO 12: Usuario decidió no compartir");
                }

                System.Diagnostics.Debug.WriteLine("🎉 ========== FIN GenerarPdf (EXITOSO) ==========");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("💥 ========== EXCEPCIÓN EN GenerarPdf ==========");
                System.Diagnostics.Debug.WriteLine($"❌ Tipo: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"❌ Mensaje: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Source: {ex.Source}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace:");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"❌ InnerException StackTrace:");
                    System.Diagnostics.Debug.WriteLine(ex.InnerException.StackTrace);
                }

                ErrorMessage = $"Error al generar PDF: {ex.Message}";

                try
                {
                    await MostrarAlerta("❌ Error Fatal",
                        $"Error al generar PDF:\n\n{ex.Message}\n\nRevisa la consola de Debug para más detalles.");
                }
                catch (Exception alertEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No se pudo mostrar alerta de error: {alertEx.Message}");
                }
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("🏁 IsLoading = false");
            }
        }
        // Método para crear datos de ejemplo
        private OrdenTrabajo CrearOrdenEjemplo()
        {
            return new OrdenTrabajo
            {
                NumeroOrden = "OT-2024-0001",
                Fecha = DateTime.Now,
                Cliente = new Cliente
                {
                    Nombre = "Juan Pérez García",
                    Telefono = "(555) 987-6543",
                    Email = "juan.perez@email.com",
                    Direccion = "Av. Reforma #456, Col. Centro"
                },
                Vehiculo = new Vehiculo
                {
                    Marca = "Toyota",
                    Modelo = "Corolla",
                    Año = "2020",
                    Color = "Plata",
                    Placas = "ABC-123-XYZ",
                    Kilometraje = "45,000 km"
                },
                Trabajos = new List<Trabajo>
                {
                    new Trabajo { Descripcion = "Cambio de aceite y filtro", Completado = true },
                    new Trabajo { Descripcion = "Rotación de neumáticos", Completado = true },
                    new Trabajo { Descripcion = "Revisión de frenos", Completado = true },
                    new Trabajo { Descripcion = "Alineación y balanceo", Completado = true },
                    new Trabajo { Descripcion = "Cambio de batería", Completado = true }
                },
                Refacciones = new List<Refaccion>
                {
                    new Refaccion { Descripcion = "Aceite sintético 5W-30 (5L)", Cantidad = 1, PrecioUnitario = 450.00m },
                    new Refaccion { Descripcion = "Filtro de aceite original", Cantidad = 1, PrecioUnitario = 120.00m },
                    new Refaccion { Descripcion = "Filtro de aire", Cantidad = 1, PrecioUnitario = 180.00m },
                    new Refaccion { Descripcion = "Batería 12V 60Ah", Cantidad = 1, PrecioUnitario = 1850.00m },
                    new Refaccion { Descripcion = "Líquido de frenos DOT 4", Cantidad = 1, PrecioUnitario = 95.00m }
                },
                ManosObra = new List<ManoObra>
                {
                    new ManoObra { Descripcion = "Cambio de aceite y filtros", Horas = 0.5m, PrecioPorHora = 200.00m },
                    new ManoObra { Descripcion = "Rotación de neumáticos", Horas = 0.5m, PrecioPorHora = 200.00m },
                    new ManoObra { Descripcion = "Revisión de sistema de frenos", Horas = 1.0m, PrecioPorHora = 200.00m },
                    new ManoObra { Descripcion = "Alineación y balanceo", Horas = 1.5m, PrecioPorHora = 200.00m },
                    new ManoObra { Descripcion = "Instalación de batería", Horas = 0.5m, PrecioPorHora = 200.00m }
                },
                Observaciones = "El vehículo fue revisado completamente. Se recomienda próximo servicio en 5,000 km o 6 meses. " +
                               "Todas las refacciones utilizadas son originales y cuentan con garantía de 6 meses."
            };
        }

        private InfoTaller CrearInfoTaller()
        {
            return new InfoTaller
            {
                Nombre = "Taller Mecánico AutoService",
                Direccion = "Calle Principal #123, Ciudad",
                Telefono = "(555) 123-4567",
                Email = "contacto@autoservice.com"
            };
        }

        private async Task AbrirPdf(string filePath)
        {
            try
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir el PDF: {ex.Message}");
            }
        }

        private async Task CompartirPdf(string filePath)
        {
            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = $"Orden de Trabajo {Orden.NumeroOrden}",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo compartir el PDF: {ex.Message}");
            }
        }
        #endregion

        #region Métodos
        private async Task VerEvidenciasTrabajo()
        {
            if (Orden == null) return;

            try
            {
                var evidenciasPage = new EvidenciasOrdenTrabajo(Orden.Id,1);
                await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);

            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }
        }
        private async Task VerRefaccionesTrabajo(int TrabajoID)
        {
            int trabajoId = TrabajoID;

            try
            {
                var refaccionesTrabajoPage = new RefaccionesTrabajo(trabajoId);
                await Application.Current.MainPage.Navigation.PushAsync(refaccionesTrabajoPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }
        }

        private async Task CargarDatosOrden()
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Iniciando carga de orden {_ordenId}");

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine("📡 Llamando API...");
                var ordenCompleta = await _apiService.ObtenerOrdenCompletaAsync(_ordenId);

                if (ordenCompleta != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Orden cargada: {ordenCompleta.NumeroOrden}");
                    Orden = ordenCompleta;
                    // ... resto del código
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ ordenCompleta es NULL");
                    ErrorMessage = "No se pudo cargar la orden";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📚 StackTrace: {ex.StackTrace}");
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("🏁 Finalizó carga");
            }
        }

        private async Task VerCliente()
        {
            if (Orden == null) return;

            try
            {
                var clientePage = new ClientesPage(Orden.ClienteId);
                await Application.Current.MainPage.Navigation.PushAsync(clientePage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir los datos del cliente: {ex.Message}");
            }
        }

        private async Task VerVehiculo()
        {
            if (Orden == null) return;

            try
            {
                var vehiculoPage = new VehiculosPage(Orden.VehiculoId);
                await Application.Current.MainPage.Navigation.PushAsync(vehiculoPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir los datos del vehículo: {ex.Message}");
            }
        }

        private async Task VerEvidencias()
        {
            if (Orden == null) return;

            try
            {
                var evidenciasPage = new EvidenciasOrdenTrabajo(Orden.Id, 2);
                await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"No se pudo abrir las evidencias de trabajo: {ex.Message}");
            }

        }

        private async Task CancelarOrden()
        {
            if (Orden == null || !PuedeCancelar) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "⚠️ Cancelar Orden",
                $"¿Estás seguro de cancelar la orden {Orden.NumeroOrden}?\n\n" +
                "Esta acción cancelará todos los trabajos asociados.",
                "Sí, cancelar",
                "No");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var response = await _apiService.CancelarOrdenAsync(Orden.Id);

                if (response.Success)
                {
                    await MostrarAlerta("✅ Éxito", "Orden cancelada correctamente");
                    await CargarDatosOrden(); // Recargar datos
                }
                else
                {
                    await MostrarAlerta("❌ Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"Error al cancelar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EntregarOrden()
        {
            if (Orden == null || !PuedeEntregar) return;

            // Verificar progreso
            if (Orden.ProgresoGeneral < 100)
            {
                await MostrarAlerta(
                    "⚠️ No se puede entregar",
                    $"La orden aún no está completada.\n\n" +
                    $"Progreso actual: {Orden.ProgresoFormateado}\n" +
                    $"Trabajos: {Orden.ProgresoTexto}");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "🚗 Entregar Vehículo",
                $"¿Confirmas la entrega del vehículo?\n\n" +
                $"Orden: {Orden.NumeroOrden}\n" +
                $"Cliente: {Orden.ClienteNombre}\n" +
                $"Vehículo: {Orden.VehiculoCompleto}",
                "Sí, entregar",
                "Cancelar");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var response = await _apiService.EntregarOrdenAsync(Orden.Id);

                if (response.Success)
                {
                    await MostrarAlerta(
                        "✅ Vehículo Entregado",
                        "El vehículo ha sido entregado correctamente.\n" +
                        "Se ha registrado en el historial.");

                    // Regresar a la página anterior
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    await MostrarAlerta("❌ Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                await MostrarAlerta("Error", $"Error al entregar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task MostrarAlerta(string titulo, string mensaje)
        {
            try
            {
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error mostrando alerta: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}