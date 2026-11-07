using TallerMecanico.Models;

namespace TallerMecanico.Services;

public static class PdfService
{
    public static async Task<string> GenerarComprobanteTurnoPdfAsync(Turno turno, List<TurnoRepuesto>? repuestos = null)
    {
        return await Task.Run(() =>
        {
            var nombreArchivo = $"Comprobante_{turno.Patente}_{DateTime.Now:yyyyMMdd_HHmm}.html";
            var rutaCompleta = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            // Armar filas de repuestos si existen
            string filasRepuestos = "";
            if (repuestos != null && repuestos.Any())
            {
                foreach (var r in repuestos)
                {
                    decimal unitario = r.Cantidad > 0 ? (r.Subtotal / r.Cantidad) : 0;
                    filasRepuestos += $@"
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #E2E8F0;'>{r.NombreRepuesto}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: center;'>{r.Cantidad}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: right;'>${unitario:N2}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: right; font-weight: bold;'>${r.Subtotal:N2}</td>
                    </tr>";
                }
            }

            string bloqueRepuestosHtml = string.IsNullOrEmpty(filasRepuestos) ? "" : $@"
                <h3 style='color: #1E40AF; margin-top: 20px; font-size: 14px; text-transform: uppercase;'>Repuestos Utilizados</h3>
                <table style='width: 100%; border-collapse: collapse; margin-top: 8px; font-size: 13px;'>
                    <thead>
                        <tr style='background-color: #F1F5F9; color: #475569;'>
                            <th style='padding: 8px; text-align: left;'>Repuesto</th>
                            <th style='padding: 8px; text-align: center;'>Cant.</th>
                            <th style='padding: 8px; text-align: right;'>Precio U.</th>
                            <th style='padding: 8px; text-align: right;'>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        {filasRepuestos}
                    </tbody>
                </table>";

            string bloqueProximoService = turno.FechaProximoService.HasValue ? $@"
                <div style='margin-top: 20px; background-color: #FEF3C7; border: 1px solid #F59E0B; border-radius: 8px; padding: 12px; color: #B45309; font-weight: bold;'>
                    🔔 PRÓXIMO SERVICE RECOMENDADO: {turno.FechaProximoService.Value:dd/MM/yyyy}
                </div>" : "";

            // Plantilla HTML del comprobante con estilo formal
            string htmlContent = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Comprobante de Servicio - {turno.Patente}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 20px; color: #1E293B; }}
        .factura-card {{ max-width: 650px; margin: auto; background: white; border-radius: 12px; padding: 30px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); border: 1px solid #E2E8F0; }}
        .header {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #E2E8F0; padding-bottom: 15px; }}
        .logo-text {{ font-size: 26px; font-weight: 800; color: #1E40AF; }}
        .grid-info {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 20px; background: #F8FAFC; padding: 15px; border-radius: 8px; border: 1px solid #E2E8F0; }}
        .info-col h4 {{ margin: 0 0 6px 0; font-size: 11px; text-transform: uppercase; color: #64748B; letter-spacing: 0.5px; }}
        .info-col p {{ margin: 3px 0; font-size: 13px; }}
        .badge-patente {{ display: inline-block; background: #1E293B; color: white; padding: 2px 8px; border-radius: 4px; font-weight: bold; font-size: 12px; }}
        .badge-estado {{ display: inline-block; background: #DCFCE7; color: #15803D; padding: 3px 8px; border-radius: 4px; font-weight: bold; font-size: 11px; }}
        .total-box {{ text-align: right; margin-top: 20px; padding-top: 15px; border-top: 2px solid #E2E8F0; }}
        .total-number {{ font-size: 24px; font-weight: 800; color: #16A34A; margin-top: 5px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 11px; color: #94A3B8; border-top: 1px solid #E2E8F0; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class='factura-card'>
        <div class='header'>
            <div>
                <div class='logo-text'>PACHITALLER</div>
                <div style='font-size: 12px; color: #64748B;'>Servicio Mecánico Integral</div>
            </div>
            <div style='text-align: right;'>
                <div style='font-weight: bold; font-size: 14px;'>Comprobante #{turno.Id:D5}</div>
                <div style='font-size: 11px; color: #64748B;'>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
                <div style='margin-top: 4px;'><span class='badge-estado'>{turno.Estado}</span></div>
            </div>
        </div>

        <div class='grid-info'>
            <div class='info-col'>
                <h4>Datos del Cliente</h4>
                <p><strong>{turno.NombreCliente}</strong></p>
                <p>Tel: {turno.TelefonoCliente}</p>
            </div>
            <div class='info-col'>
                <h4>Datos del Vehículo</h4>
                <p><span class='badge-patente'>{turno.Patente}</span> {turno.MarcaVehiculo} {turno.ModeloVehiculo}</p>
                <p>Tipo: {turno.TipoServicio}</p>
            </div>
        </div>

        <div style='margin-top: 20px;'>
            <h4 style='font-size: 11px; text-transform: uppercase; color: #64748B; margin-bottom: 5px;'>Detalle del Servicio</h4>
            <p style='margin: 4px 0; font-size: 13px;'><strong>Falla reportada:</strong> {turno.Problema}</p>
            <p style='margin: 4px 0; font-size: 13px;'><strong>Diagnóstico técnico:</strong> {turno.Diagnostico}</p>
        </div>

        {bloqueRepuestosHtml}

        <div class='total-box'>
            <div style='font-size: 13px; color: #475569;'>Mano de Obra: <strong>${turno.ManoObra:N2}</strong></div>
            <div class='total-number'>TOTAL FINAL: ${turno.CostoTotal:N2}</div>
        </div>

        {bloqueProximoService}

        <div class='footer'>
            PachiTaller - Garantía de confianza y repuestos de primera calidad.<br>
            ¡Gracias por su preferencia!
        </div>
    </div>
</body>
</html>";

            File.WriteAllText(rutaCompleta, htmlContent);
            return rutaCompleta;
        });
    }
    public static async Task<string> GenerarTicketVentaRepuestosAsync(List<RepuestoVentaItem> items, decimal total)
    {
        return await Task.Run(() =>
        {
            var nombreArchivo = $"Ticket_Venta_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var rutaCompleta = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            string filas = "";
            foreach (var item in items)
            {
                filas += $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #E2E8F0;'>{item.Nombre}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: center;'>{item.CantidadVenta}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: right;'>${item.PrecioUnitario:N2}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #E2E8F0; text-align: right; font-weight: bold;'>${item.Subtotal:N2}</td>
                </tr>";
            }

            string html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Ticket de Venta - PachiTaller</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #F8FAFC; margin: 0; padding: 20px; }}
        .ticket-card {{ max-width: 500px; margin: auto; background: white; border-radius: 12px; padding: 25px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); border: 1px solid #E2E8F0; }}
        .header {{ text-align: center; border-bottom: 2px dashed #CBD5E1; padding-bottom: 15px; }}
        .logo {{ font-size: 24px; font-weight: 800; color: #1E40AF; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 13px; }}
        th {{ background: #F1F5F9; color: #475569; padding: 8px; text-align: left; }}
        .total-box {{ text-align: right; margin-top: 20px; border-top: 2px dashed #CBD5E1; padding-top: 15px; }}
        .total-number {{ font-size: 22px; font-weight: 800; color: #16A34A; }}
        .footer {{ text-align: center; margin-top: 25px; font-size: 11px; color: #94A3B8; }}
    </style>
</head>
<body>
    <div class='ticket-card'>
        <div class='header'>
            <div class='logo'>PACHITALLER</div>
            <div style='font-size: 12px; color: #64748B;'>Venta de Repuestos y Accesorios</div>
            <div style='font-size: 11px; color: #94A3B8; margin-top: 5px;'>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</div>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Artículo</th>
                    <th style='text-align: center;'>Cant.</th>
                    <th style='text-align: right;'>Precio</th>
                    <th style='text-align: right;'>Subtotal</th>
                </tr>
            </thead>
            <tbody>
                {filas}
            </tbody>
        </table>

        <div class='total-box'>
            <div class='total-number'>TOTAL: ${total:N2}</div>
        </div>

        <div class='footer'>
            ¡Muchas gracias por su compra!<br>
            PachiTaller - Repuestos con garantía.
        </div>
    </div>
</body>
</html>";

            File.WriteAllText(rutaCompleta, html);
            return rutaCompleta;
        });
    }
}