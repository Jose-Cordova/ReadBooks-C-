using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;
using System.ComponentModel.DataAnnotations;
using System.Reflection;


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
        public async Task<IActionResult> Create(int LibroId, int EstudianteId, string CondicionAlEntregar, int Cantidad, DateTime FechaDevolucionProgramada)
        {
            
            if (LibroId <= 0 || EstudianteId <= 0 || string.IsNullOrWhiteSpace(CondicionAlEntregar) || Cantidad <= 0)
            {
                return Json(new { success = false, message = "Verifica que todos los campos estén completos." });
            }

           
            var libro = await _context.Libros.FindAsync(LibroId);
            if (libro == null)
            {
                return Json(new { success = false, message = "El libro seleccionado no es válido." });
            }

           
            var estudiante = await _context.Estudiantes.FindAsync(EstudianteId);
            if (estudiante == null)
            {
                return Json(new { success = false, message = "El estudiante seleccionado no es válido." });
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

            prestamo.CondicionAlDevolver = ObtenerNombreDisplay(condicionAlDevolver);
            prestamo.Estado = EstadoPrestamo.DEVUELTO;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Devolución Realizada con éxito " });
        }
        private static string ObtenerNombreDisplay(CondicionLibro condicion)
        {
            var miembro = condicion.GetType().GetMember(condicion.ToString());
            var atributo = miembro[0].GetCustomAttribute<DisplayAttribute>();
            return atributo?.Name ?? condicion.ToString();
        }

    }
}
