using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;


namespace ReadBooks.Controllers
{

   
    public class PrestamoController : Controller
    {
        private readonly ReadBooksDbContext _context;

        public PrestamoController(ReadBooksDbContext context)
        {
            _context = context; 
        }


        // get para prestamo
        public async Task<IActionResult> Index()
        {
            var prestamos = await _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Estudiante)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(prestamos);
        }

        //get : prestamo, para crear nuevo prestamo
        public async Task<IActionResult> Create()
        {
            ViewBag.Libros = await _context.Libros
                .Where(l => l.EjemplaresDisponibles > 0)
                .OrderBy(l => l.Titulo)
                .ToListAsync();

            ViewBag.Estudiantes = await _context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return PartialView("Create", new Prestamo());
        }

        // post : 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string LibroTitulo, string EstudianteNombre, string CondicionAlEntregar, int Cantidad, DateTime FechaDevolucionProgramada)
        {
            if (string.IsNullOrWhiteSpace(LibroTitulo) || string.IsNullOrWhiteSpace(EstudianteNombre) ||
                string.IsNullOrWhiteSpace(CondicionAlEntregar) || Cantidad <= 0)
            {
                return Json(new { success = false, message = "Verifica que todos los campos estén completos." });
            }

            var libro = await _context.Libros
                .FirstOrDefaultAsync(l => l.Titulo == LibroTitulo);

            if (libro == null)
            {
                return Json(new { success = false, message = "El libro escrito no coincide con ninguno registrado. Selecciónalo de la lista." });
            }

            // Extrae el carnet del texto 
            string carnet = "";
            int idxInicio = EstudianteNombre.LastIndexOf('(');
            int idxFin = EstudianteNombre.LastIndexOf(')');
            if (idxInicio >= 0 && idxFin > idxInicio)
            {
                carnet = EstudianteNombre.Substring(idxInicio + 1, idxFin - idxInicio - 1).Trim();
            }

            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Carnet == carnet);

            if (estudiante == null)
            {
                return Json(new { success = false, message = "El estudiante escrito no coincide con ninguno registrado. Selecciónalo de la lista." });
            }

            if (!libro.DescontarEjemplares(Cantidad))
            {
                return Json(new { success = false, message = "No hay suficientes ejemplares disponibles de ese libro." });
            }

            var prestamo = new Prestamo
            {
                LibroId = libro.Id,
                EstudianteId = estudiante.Id,
                CondicionAlEntregar = CondicionAlEntregar,
                CondicionAlDevolver = "",
                Cantidad = Cantidad,
                FechaDevolucionProgramada = FechaDevolucionProgramada,
                Fecha = DateTime.Now,
                Estado = EstadoPrestamo.PRESTADO
            };

            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Préstamo registrado correctamente." });
        }

        //get: prestamo, registra la devolución, muestra el modal
        public async Task<IActionResult> RegistrarDevolucion(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Estudiante)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if ( prestamo == null)
            {
                return NotFound();
            }

            return PartialView("_RegistrarDevolucionModal", prestamo);
        }

        //post: confima la devolucion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarDevolucion(int prestamoId, CondicionLibro condicionAlDevolver)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(p => p.Id == prestamoId);

            if (prestamo == null)
            {
                return Json(new { success = false, message = "Este prestamo no existe. " });
            }

            if (prestamo.Libro == null || !prestamo.Libro.AumentarEjemplares(prestamo.Cantidad))
            {
                return Json(new { success = false, message = "No se pudo actualizar el stock del libro. " });
            }

            prestamo.CondicionAlDevolver = condicionAlDevolver.ToString();
            prestamo.Estado = EstadoPrestamo.DEVUELTO;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Devolución Realizada con éxito " });
        }
    }
}
