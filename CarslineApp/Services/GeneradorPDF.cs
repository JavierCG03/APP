using CarslineApp.Models;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iTextCell = iText.Layout.Element.Cell;
using iTextHorizontalAlignment = iText.Layout.Properties.HorizontalAlignment;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTable = iText.Layout.Element.Table;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;

namespace CarslineApp.Services
{
    public class PdfGeneratorService
    {
        /// <summary>
        /// Genera un PDF de orden de trabajo y devuelve la ruta del archivo
        /// </summary>
        public async Task<string> GenerarOrdenTrabajoPDF(OrdenTrabajo orden, InfoTaller taller)
        {
            string fileName = $"OrdenTrabajo_{orden.NumeroOrden}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

#if ANDROID
            var docsDirectory = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments);
            var filePath = Path.Combine(docsDirectory.AbsoluteFile.Path, fileName);
#else
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
#endif

            await Task.Run(() => GenerarPDF(filePath, orden, taller));
            return filePath;
        }

        private void GenerarPDF(string filePath, OrdenTrabajo orden, InfoTaller taller)
        {
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                AgregarEncabezado(document, orden, taller);
                AgregarDatosCliente(document, orden.Cliente);
                AgregarDatosVehiculo(document, orden.Vehiculo);
                AgregarTrabajosRealizados(document, orden.Trabajos);
                AgregarRefacciones(document, orden.Refacciones, orden.TotalRefacciones);
                AgregarManoDeObra(document, orden.ManosObra, orden.TotalManoObra);
                AgregarResumenCostos(document, orden);
                AgregarPiePagina(document, orden.Observaciones);

                document.Close();
            }
        }

        private void AgregarEncabezado(Document document, OrdenTrabajo orden, InfoTaller taller)
        {
            iTextParagraph titulo = new iTextParagraph("ORDEN DE TRABAJO")
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetFontSize(24)
                .SetBold()
                .SetFontColor(new DeviceRgb(41, 128, 185));

            document.Add(titulo);

            iTextParagraph infoTaller = new iTextParagraph($"{taller.Nombre}\n{taller.Direccion}\nTel: {taller.Telefono}")
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginBottom(10);

            document.Add(infoTaller);

            iTextTable headerTable = new iTextTable(2);
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));

            headerTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"No. Orden: {orden.NumeroOrden}").SetBold())
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iTextTextAlignment.LEFT));

            headerTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"Fecha: {orden.Fecha:dd/MM/yyyy}").SetBold())
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iTextTextAlignment.RIGHT));

            document.Add(headerTable);

            LineSeparator separator = new LineSeparator(new SolidLine());
            document.Add(separator);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarDatosCliente(Document document, Cliente cliente)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("DATOS DEL CLIENTE")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable clienteTable = new iTextTable(2);
            clienteTable.SetWidth(UnitValue.CreatePercentValue(100));

            var bgColor = new DeviceRgb(236, 240, 241);

            clienteTable.AddCell(CreateDataCell("Nombre:", true, bgColor));
            clienteTable.AddCell(CreateDataCell(cliente.Nombre, false, null));

            clienteTable.AddCell(CreateDataCell("Teléfono:", true, bgColor));
            clienteTable.AddCell(CreateDataCell(cliente.Telefono, false, null));

            clienteTable.AddCell(CreateDataCell("Email:", true, bgColor));
            clienteTable.AddCell(CreateDataCell(cliente.Email, false, null));

            clienteTable.AddCell(CreateDataCell("Dirección:", true, bgColor));
            clienteTable.AddCell(CreateDataCell(cliente.Direccion, false, null));

            document.Add(clienteTable);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarDatosVehiculo(Document document, Vehiculo vehiculo)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("DATOS DEL VEHÍCULO")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable vehiculoTable = new iTextTable(new float[] { 1, 1, 1 });
            vehiculoTable.SetWidth(UnitValue.CreatePercentValue(100));

            var bgColor = new DeviceRgb(236, 240, 241);

            vehiculoTable.AddCell(CreateDataCell("Marca:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Marca, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            vehiculoTable.AddCell(CreateDataCell("Modelo:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Modelo, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            vehiculoTable.AddCell(CreateDataCell("Año:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Año, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            vehiculoTable.AddCell(CreateDataCell("Color:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Color, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            vehiculoTable.AddCell(CreateDataCell("Placas:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Placas, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            vehiculoTable.AddCell(CreateDataCell("Kilometraje:", true, bgColor));
            vehiculoTable.AddCell(CreateDataCell(vehiculo.Kilometraje, false, null));
            vehiculoTable.AddCell(new iTextCell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            document.Add(vehiculoTable);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarTrabajosRealizados(Document document, List<Trabajo> trabajos)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("TRABAJOS REALIZADOS")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable trabajosTable = new iTextTable(new float[] { 3, 1 });
            trabajosTable.SetWidth(UnitValue.CreatePercentValue(100));

            trabajosTable.AddHeaderCell(CreateHeaderCell("Descripción del Trabajo"));
            trabajosTable.AddHeaderCell(CreateHeaderCell("Estado"));

            foreach (var trabajo in trabajos)
            {
                trabajosTable.AddCell(CreateDataCell(trabajo.Descripcion, false, null));

                var estadoColor = trabajo.Completado ? new DeviceRgb(39, 174, 96) : new DeviceRgb(230, 126, 34);
                trabajosTable.AddCell(CreateDataCell(trabajo.Estado, false, estadoColor));
            }

            document.Add(trabajosTable);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarRefacciones(Document document, List<Refaccion> refacciones, decimal total)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("REFACCIONES UTILIZADAS")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable refaccionesTable = new iTextTable(new float[] { 3, 1, 1, 1.5f });
            refaccionesTable.SetWidth(UnitValue.CreatePercentValue(100));

            refaccionesTable.AddHeaderCell(CreateHeaderCell("Descripción"));
            refaccionesTable.AddHeaderCell(CreateHeaderCell("Cant."));
            refaccionesTable.AddHeaderCell(CreateHeaderCell("Precio Unit."));
            refaccionesTable.AddHeaderCell(CreateHeaderCell("Subtotal"));

            foreach (var refaccion in refacciones)
            {
                AgregarFilaRefaccion(refaccionesTable,
                    refaccion.Descripcion,
                    refaccion.Cantidad.ToString(),
                    $"${refaccion.PrecioUnitario:N2}",
                    $"${refaccion.Subtotal:N2}");
            }

            refaccionesTable.AddCell(new iTextCell(1, 3)
                .Add(new iTextParagraph("TOTAL REFACCIONES:").SetBold())
                .SetTextAlignment(iTextTextAlignment.RIGHT)
                .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                .SetFontColor(ColorConstants.WHITE));

            refaccionesTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"${total:N2}").SetBold())
                .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                .SetFontColor(ColorConstants.WHITE));

            document.Add(refaccionesTable);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarManoDeObra(Document document, List<ManoObra> manosObra, decimal total)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("MANO DE OBRA")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable manoObraTable = new iTextTable(new float[] { 3, 1, 1, 1.5f });
            manoObraTable.SetWidth(UnitValue.CreatePercentValue(100));

            manoObraTable.AddHeaderCell(CreateHeaderCell("Descripción"));
            manoObraTable.AddHeaderCell(CreateHeaderCell("Horas"));
            manoObraTable.AddHeaderCell(CreateHeaderCell("Precio/Hora"));
            manoObraTable.AddHeaderCell(CreateHeaderCell("Subtotal"));

            foreach (var manoObra in manosObra)
            {
                AgregarFilaRefaccion(manoObraTable,
                    manoObra.Descripcion,
                    manoObra.Horas.ToString("N1"),
                    $"${manoObra.PrecioPorHora:N2}",
                    $"${manoObra.Subtotal:N2}");
            }

            manoObraTable.AddCell(new iTextCell(1, 3)
                .Add(new iTextParagraph("TOTAL MANO DE OBRA:").SetBold())
                .SetTextAlignment(iTextTextAlignment.RIGHT)
                .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                .SetFontColor(ColorConstants.WHITE));

            manoObraTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"${total:N2}").SetBold())
                .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                .SetFontColor(ColorConstants.WHITE));

            document.Add(manoObraTable);
            document.Add(new iTextParagraph("\n").SetFontSize(8));
        }

        private void AgregarResumenCostos(Document document, OrdenTrabajo orden)
        {
            iTextParagraph seccionTitulo = new iTextParagraph("RESUMEN DE COSTOS")
                .SetFontSize(14)
                .SetBold()
                .SetFontColor(new DeviceRgb(52, 73, 94))
                .SetMarginBottom(5);

            document.Add(seccionTitulo);

            iTextTable resumenTable = new iTextTable(new float[] { 3, 1.5f });
            resumenTable.SetWidth(UnitValue.CreatePercentValue(60))
                .SetHorizontalAlignment(iTextHorizontalAlignment.RIGHT);

            resumenTable.AddCell(CreateDataCell("Subtotal Refacciones:", true, new DeviceRgb(236, 240, 241)));
            resumenTable.AddCell(CreateDataCell($"${orden.TotalRefacciones:N2}", false, null));

            resumenTable.AddCell(CreateDataCell("Subtotal Mano de Obra:", true, new DeviceRgb(236, 240, 241)));
            resumenTable.AddCell(CreateDataCell($"${orden.TotalManoObra:N2}", false, null));

            resumenTable.AddCell(CreateDataCell("Subtotal:", true, new DeviceRgb(236, 240, 241)));
            resumenTable.AddCell(CreateDataCell($"${orden.Subtotal:N2}", false, null));

            resumenTable.AddCell(CreateDataCell("IVA (16%):", true, new DeviceRgb(236, 240, 241)));
            resumenTable.AddCell(CreateDataCell($"${orden.IVA:N2}", false, null));

            resumenTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("TOTAL A PAGAR:").SetBold().SetFontSize(12))
                .SetBackgroundColor(new DeviceRgb(231, 76, 60))
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(8));

            resumenTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"${orden.Total:N2}").SetBold().SetFontSize(12))
                .SetBackgroundColor(new DeviceRgb(231, 76, 60))
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(8));

            document.Add(resumenTable);
            document.Add(new iTextParagraph("\n\n").SetFontSize(10));
        }

        private void AgregarPiePagina(Document document, string observaciones)
        {
            iTextParagraph obsTitle = new iTextParagraph("OBSERVACIONES:")
                .SetFontSize(10)
                .SetBold()
                .SetMarginBottom(5);

            document.Add(obsTitle);

            iTextParagraph textoObservaciones = new iTextParagraph(observaciones)
                .SetFontSize(9)
                .SetMarginBottom(15);

            document.Add(textoObservaciones);

            iTextTable firmasTable = new iTextTable(2);
            firmasTable.SetWidth(UnitValue.CreatePercentValue(100));

            firmasTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("\n\n\n_________________________\nFirma del Cliente").SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            firmasTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("\n\n\n_________________________\nFirma del Mecánico").SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            document.Add(firmasTable);

            iTextParagraph textofinal = new iTextParagraph("\nGracias por su preferencia")
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetFontSize(10)
                .SetItalic()
                .SetFontColor(ColorConstants.GRAY);

            document.Add(textofinal);
        }

        // Métodos auxiliares
        private iTextCell CreateHeaderCell(string text)
        {
            return new iTextCell()
                .Add(new iTextParagraph(text).SetBold().SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(52, 73, 94))
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetPadding(5);
        }

        private iTextCell CreateDataCell(string text, bool isBold, DeviceRgb backgroundColor)
        {
            iTextParagraph p = new iTextParagraph(text).SetFontSize(10);

            if (isBold)
                p.SetBold();

            iTextCell cell = new iTextCell().Add(p).SetPadding(5);

            if (backgroundColor != null)
                cell.SetBackgroundColor(backgroundColor);

            return cell;
        }

        private void AgregarFilaRefaccion(iTextTable table, string descripcion, string cantidad, string precioUnit, string subtotal)
        {
            table.AddCell(CreateDataCell(descripcion, false, null));
            table.AddCell(CreateDataCell(cantidad, false, null).SetTextAlignment(iTextTextAlignment.CENTER));
            table.AddCell(CreateDataCell(precioUnit, false, null).SetTextAlignment(iTextTextAlignment.RIGHT));
            table.AddCell(CreateDataCell(subtotal, false, null).SetTextAlignment(iTextTextAlignment.RIGHT));
        }
    }
}