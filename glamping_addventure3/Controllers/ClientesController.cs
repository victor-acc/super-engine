using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using glamping_addventure3.Models;
using Microsoft.AspNetCore.Authorization;

namespace reserva3.Controllers
{
    [Authorize]
    [Authorize(Policy = "AccederClientes")]
    public class ClientesController : Controller
    {
        private readonly GlampingAddventure3Context _context;

        public ClientesController(GlampingAddventure3Context context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {

            var glamping3Context = _context.Clientes.Include(c => c.IdrolNavigation);
            return View(await glamping3Context.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdrolNavigation)
                .FirstOrDefaultAsync(m => m.NroDocumento == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            ViewData["Idrol"] = new SelectList(_context.Roles, "Idrol", "Idrol");
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NroDocumento,TipoDocumento,Nombre,Apellido,Direccion,Email,Telefono,Estado,Idrol")] Cliente cliente)
        {
            // Validar si el número de documento ya existe
            if (_context.Clientes.Any(c => c.NroDocumento == cliente.NroDocumento))
            {
                ModelState.AddModelError("NroDocumento", "El número de documento ya está registrado.");
            }

            // Validar si el correo ya existe
            if (_context.Clientes.Any(c => c.Email == cliente.Email))
            {
                ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
            }

            // Validar si el teléfono ya existe
            if (_context.Clientes.Any(c => c.Telefono == cliente.Telefono))
            {
                ModelState.AddModelError("Telefono", "El número de teléfono ya está registrado.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Redirigir a la vista de creación de reserva
                return RedirectToAction("Create", "Reservas");
            }

            // Si alguna validación falla, se muestra el formulario nuevamente con los errores
            ViewData["Idrol"] = new SelectList(_context.Roles, "Idrol", "Idrol", cliente.Idrol);
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            ViewData["Idrol"] = new SelectList(_context.Roles, "Idrol", "Idrol", cliente.Idrol);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NroDocumento,,Nombre,Apellido,Direccion,Email,Telefono,Estado,Idrol")] Cliente cliente)
        {
            if (id != cliente.NroDocumento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.NroDocumento))
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
            ViewData["Idrol"] = new SelectList(_context.Roles, "Idrol", "Idrol", cliente.Idrol);
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        [HttpGet]
        // GET: Clientes/Delete/12345678
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdrolNavigation)
                .FirstOrDefaultAsync(m => m.NroDocumento == id);

            if (cliente == null)
            {
                return NotFound();
            }

            // Devolver una vista parcial para confirmar la eliminación
            return PartialView("_DeletePartial", cliente);
        }

        [HttpPost]
        // POST: Clientes/DeleteConfirmed
        public JsonResult DeleteConfirmed(string numero)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.NroDocumento == numero);
            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente no encontrado." });
            }

            // Verificar si el cliente está asociado a una reserva
            var asociadoAPaquete = _context.Reservas.Any(h => h.NroDocumentoCliente == numero);

            if (asociadoAPaquete)
            {
                return Json(new { success = false, message = "No se puede eliminar porque este cliente está asociado a una reserva." });
            }

            // Eliminar el cliente
            try
            {
                _context.Clientes.Remove(cliente);
                _context.SaveChanges();
                return Json(new { success = true, message = "Cliente eliminado con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el cliente: " + ex.Message });
            }
        }


        private bool ClienteExists(string id)
        {
            return _context.Clientes.Any(e => e.NroDocumento == id);
        }
    }
}
