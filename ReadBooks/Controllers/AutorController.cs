using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;
using System.Threading.Tasks;

namespace ReadBooks.Controllers
{
    public class AutorController : Controller
    {
        private readonly ReadBooksDbContext _context;

        public AutorController(ReadBooksDbContext context)
        {
            _context = context;
        }

        //Obtener todos los autores
        public async Task<IActionResult> Index()
        {
            var autores = await _context.Autores.AsNoTracking().ToListAsync();
            return View(autores);
        }

        //Obtener solo un autor
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var autor = await _context.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if(autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        //Mostrar formulario para crear autor
        public IActionResult Create()
        {
            return View();
        }

        //Crear autor
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Autor autor)
        {
            if(autor.Nombre != null)
            {
                autor.Nombre = autor.Nombre.Trim();
            }

            if(!string.IsNullOrEmpty(autor.Nombre) &&
                await _context.Autores.AnyAsync(a => a.Nombre.ToLower() == autor.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe un autor con este nombre.");
            }

            if(ModelState.IsValid)
            {
                _context.Add(autor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(autor);
        }

        //Mostrar formulario con datos para editar
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var autor = await _context.Autores.FindAsync(id);
            if(autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        //Actualizar autor
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Autor autor)
        {
            if(id != autor.Id)
            {
                return NotFound();
            }

            if(autor.Nombre != null)
            {
                autor.Nombre = autor.Nombre.Trim();
            }

            if(!string.IsNullOrEmpty(autor.Nombre) &&
                await _context.Autores.AnyAsync(a => a.Id != autor.Id && a.Nombre.ToLower() == autor.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe un autor con este nombre.");
            }

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(autor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if(!await AutorExistsAsync(autor.Id))
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
            return View(autor);
        }

        //Metodo asincrono para verificar si un autor existe
        private async Task<bool> AutorExistsAsync(int id)
        {
            return await _context.Autores.AnyAsync(e => e.Id == id);
        }

        //Mostrar confirmación para eliminar
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var autor = await _context.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if(autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        //Eliminar autor
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            if(autor == null)
            {
                return NotFound();
            }
            try
            {
                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException e) when (e.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503")
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar este autor porque tiene libros asociados.");
                return View(autor);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el autor. Inténtalo de nuevo.");
                return View(autor);
            }
        }
    }
}
