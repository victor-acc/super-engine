using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using glamping_addventure3.Models;
using Microsoft.AspNetCore.Authorization;

namespace glamping_addventure3.Controllers
{
    [Authorize]
    [Authorize(Policy = "AccederAbonos")]

    public class AbonoesController : Controller
    {
        private readonly GlampingAddventure3Context _context;

        public AbonoesController(GlampingAddventure3Context context)
        {
            _context = context;
        }

        // GET: Abonoes
        public async Task<IActionResult> Index(int? idReserva)
        {
            if (idReserva == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.NroDocumentoClienteNavigation) // Incluir el cliente relacionado
                .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

            if (reserva == null)
            {
                return NotFound();
            }

            var abonos = _context.Abonos
                .Include(a => a.IdreservaNavigation)
                .Where(a => a.Idreserva == idReserva);

            ViewData["IdReservaActual"] = idReserva;
            ViewData["NombreCliente"] = reserva.NroDocumentoClienteNavigation.Nombre; // Pasar el nombre del cliente a la vista
            return View(await abonos.ToListAsync());
        }

        public IActionResult GenerarPDF(int id)
        {
            // Obtener el abono desde la base de datos
            var abono = _context.Abonos
                .Include(a => a.IdreservaNavigation)
                .FirstOrDefault(a => a.Idabono == id);

            if (abono == null)
            {
                return NotFound();
            }

            // Crear un nuevo documento PDF
            var document = new PdfDocument();
            document.Info.Title = $"Detalle del Abono {abono.Idabono}";

            // Agregar una página al documento
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            // Definir la fuente con estilos correctos
            var fontRegular = new XFont("Arial", 12);
            var fontBold = new XFont("Arial", 16);

            // Escribir contenido en el PDF
            int yPosition = 40; // Coordenada Y inicial

            graphics.DrawString("Detalle del Abono", fontBold, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 40;

            graphics.DrawString($"ID del Abono: {abono.Idabono}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"ID de la Reserva: {abono.Idreserva}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Fecha de Abono: {abono.FechaAbono:yyyy-MM-dd}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Cantidad del Abono: {abono.CantAbono:C}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Valor Deuda: {abono.ValorDeuda:C}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Porcentaje: {abono.Porcentaje}%", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Pendiente: {abono.Pendiente:C}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 20;

            graphics.DrawString($"Estado: {(abono.Estado ? "Activo" : "Anulado")}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 40;

            // Verificar si existe un comprobante
            if (abono.Comprobante != null && abono.Comprobante.Length > 0)
            {
                try
                {
                    // Crear un MemoryStream desde los bytes del comprobante
                    using (var ms = new MemoryStream(abono.Comprobante))
                    {
                        // Cargar la imagen desde el MemoryStream
                        var comprobanteImage = XImage.FromStream(ms);

                        // Ajustar dimensiones y posición de la imagen en el PDF
                        double imageWidth = 200; // Ancho deseado
                        double imageHeight = comprobanteImage.PixelHeight * (imageWidth / comprobanteImage.PixelWidth); // Mantener proporción

                        // Dibujar la imagen en el PDF
                        graphics.DrawImage(comprobanteImage, 50, yPosition, imageWidth, imageHeight);
                    }
                }
                catch (Exception ex)
                {
                    graphics.DrawString($"Error al cargar el comprobante: {ex.Message}", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
                }
            }
            else
            {
                graphics.DrawString("Comprobante no disponible.", fontRegular, XBrushes.Black, new XPoint(50, yPosition));
            }

            // Guardar el PDF en memoria y devolverlo como descarga
            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                return File(stream.ToArray(), "application/pdf", $"Abono_{abono.Idabono}.pdf");
            }
        }
        // GET: Abonoes/Details/5
        public async Task<IActionResult> Details(int? id, int? idReserva)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abono = await _context.Abonos
                .Include(a => a.IdreservaNavigation)
                .FirstOrDefaultAsync(m => m.Idabono == id);

            if (abono == null)
            {
                return NotFound();
            }
            ViewData["IdReserva"] = idReserva;

            ViewData["Comprobante"] = abono.Comprobante != null
                ? $"data:image/png;base64,{Convert.ToBase64String(abono.Comprobante)}"
                : null;
            return PartialView("_DetailsPartial", abono);
        }

        // GET: Abonoes/Create
        public IActionResult Create(int? idReserva)
        {
            if (idReserva == null)
            {
                return NotFound();
            }

            var reserva = _context.Reservas.Find(idReserva);
            if (reserva == null)
            {
                return NotFound();
            }

            ViewData["IdReserva"] = idReserva;
            ViewData["MontoTotal"] = reserva.MontoTotal; // Suponiendo que la tabla Reserva tiene un campo MontoTotal.
            return View();
        }


        // POST: Abonoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idabono,Idreserva,FechaAbono,ValorDeuda,Porcentaje,Pendiente,CantAbono,Estado")] Abono abono, IFormFile comprobante)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Abonos)
                .FirstOrDefaultAsync(r => r.IdReserva == abono.Idreserva);

            if (reserva == null)
            {
                ModelState.AddModelError("", "La reserva no existe.");
                return View(abono);
            }

            var totalAbonosPrevios = reserva.Abonos
                .Where(a => !a.Estado) // Solo incluir abonos activos
                .Sum(a => a.CantAbono);

            abono.Pendiente = reserva.MontoTotal - (totalAbonosPrevios + abono.CantAbono);
            abono.Porcentaje = (abono.CantAbono / reserva.MontoTotal) * 100;

            if (totalAbonosPrevios == 0 && abono.Porcentaje < 50)
            {
                ModelState.AddModelError("", "El primer abono debe ser al menos el 50% del monto total.");
            }

            if ((totalAbonosPrevios + abono.CantAbono) > reserva.MontoTotal)
            {
                ModelState.AddModelError("", "El abono excede el monto total de la reserva.");
            }

            if (abono.CantAbono <= 0)
            {
                ModelState.AddModelError("", "El monto del abono debe ser mayor a 0.");
            }

            if (comprobante != null && comprobante.Length > 0)
            {
                if (comprobante.ContentType != "image/png" && comprobante.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("", "Solo se permiten archivos PNG o JPEG.");
                }
                else if (comprobante.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "El tamaño del comprobante no debe exceder los 2 MB.");
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await comprobante.CopyToAsync(memoryStream);
                        abono.Comprobante = memoryStream.ToArray();
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(abono);

                var totalAbonoActualizado = totalAbonosPrevios + abono.CantAbono;

                const int Confirmado = 3;
                const int PorConfirmar = 2;

                if (totalAbonoActualizado >= reserva.MontoTotal)
                {
                    reserva.IdEstadoReserva = Confirmado;
                }
                else if (totalAbonoActualizado > 0)
                {
                    reserva.IdEstadoReserva = PorConfirmar;
                }

                _context.Update(reserva);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { idReserva = abono.Idreserva });
            }

            ViewData["IdReserva"] = abono.Idreserva;
            ViewData["MontoTotal"] = reserva.MontoTotal;

            return View(abono);
        }
        // POST: Abonoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Idabono,Idreserva,FechaAbono,ValorDeuda,Porcentaje,Pendiente,CantAbono,Comprobante,Estado")] Abono abono)
        {
            if (id != abono.Idabono)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(abono);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (abono == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Idreserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", abono.Idreserva);
            return View(abono);
        }

        // GET: Abonoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abono = await _context.Abonos
                .Include(a => a.IdreservaNavigation)
                .FirstOrDefaultAsync(m => m.Idabono == id);
            if (abono == null)
            {
                return NotFound();
            }

            return View(abono);
        }

        // POST: Abonoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono != null)
            {
                _context.Abonos.Remove(abono);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Abonos/Anular/5
        // POST: Abonos/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            // Buscar el abono por su ID
            var abono = await _context.Abonos
                .Include(a => a.IdreservaNavigation) // Incluir la relación con la reserva
                .FirstOrDefaultAsync(a => a.Idabono == id);

            if (abono == null)
            {
                return NotFound();
            }

            // Marcar el abono como anulado
            abono.Estado = true;

            // Cancelar la cantidad abonada (sumarla de vuelta a la deuda)
            if (abono.IdreservaNavigation != null)
            {
                abono.IdreservaNavigation.MontoTotal = abono.CantAbono += abono.Pendiente;
            }

            try
            {
                _context.Update(abono);
                if (abono.IdreservaNavigation != null)
                {
                    _context.Update(abono.IdreservaNavigation);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al anular abono: {ex.Message}");
                ModelState.AddModelError("", "Se produjo un error al anular el abono.");
            }

            // Redirigir a la lista de abonos
            return RedirectToAction("Index", new { idReserva = abono.IdreservaNavigation?.IdReserva });
        }

    }
}
