using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;

namespace ReadBooks.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ReadBooksDbContext _context;

        public EstudiantesController(ReadBooksDbContext context)
        {
            _context = context;
        }

        //Accion para hacer uqe funcione el filtrp
        [HttpGet]
        public async Task<IActionResult> Buscar(string buscar)
        {
            if (string.IsNullOrWhiteSpace(buscar))
            {
                var todos = await _context.Estudiantes
                     .OrderBy(e => e.Nombre)
                     .ToListAsync();
                return PartialView("_TablaEstudiantes", todos);
            }

            //paso el texto a minsuculas
            var criterio = buscar.ToLower().Trim();

            //aqui va e filtro
            var filtrados = await _context.Estudiantes
                .Where(e => e.Nombre.ToLower().Contains(criterio) ||
                e.Apellido.ToLower().Contains(criterio) ||
                e.Carnet.ToLower().Contains(criterio))
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return PartialView("_TablaEstudiantes", filtrados);

        }
        // GET: Estudiantes
        public async Task<IActionResult> Index()
        {
            var estudiantes = await _context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(estudiantes);
        }


        // en este get , me trae solo el diselo del modal
        public IActionResult Create()
        {
            return PartialView();
        }

        // post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Estudiante estudiante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estudiante);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Estudiante registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }


            Response.StatusCode = 400;
            return PartialView(estudiante);
        }



        // GET: Estudiantes
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            return PartialView(estudiante);
        }

        // para editar al estudiante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Estudiante estudiante)
        {
            if (id != estudiante.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(estudiante);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Estudiante actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstudianteExists(estudiante.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }


            Response.StatusCode = 400;
            return PartialView(estudiante);
        }



        // get, obetemos los daatos, para eliminarlo
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            //aqui verifico si tien prestamos asociado
            ViewBag.TienePrestamos = await _context.Prestamos.AnyAsync(p => p.EstudianteId == id);

            return PartialView(estudiante);
        }

        // lo eliminaos ded confirmacion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var tienePrestamos = await _context.Prestamos.AnyAsync(p => p.EstudianteId == id);
            if (tienePrestamos)
            {
                TempData["Error"] = "No se puede elminar el estudiante porque tiene prestamos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Estudiante elminado correctamente. ";

            return RedirectToAction(nameof(Index));
        }

        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.Id == id);
        }
    }
}
