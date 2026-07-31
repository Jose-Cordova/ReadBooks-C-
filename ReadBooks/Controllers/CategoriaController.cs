using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;
using System.Threading.Tasks;

namespace ReadBooks.Controllers
{
    //[Authorize] //Solo administradores
    public class CategoriaController : Controller
    {
        private readonly ReadBooksDbContext _context;
        public CategoriaController(ReadBooksDbContext context)
        {
            _context = context;
        }

        //Obtener todas las categorias
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return View(categorias);
        }

        //Obtener solo una categoria
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if(categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        //Mostrar la vista del formulario vasio
        public IActionResult Create()
        {
            return View();
        }
        //Crear la categoria
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria categoria)
        {
            if (categoria.Nombre != null)
            {
                categoria.Nombre = categoria.Nombre.Trim();
            }

            if (!string.IsNullOrEmpty(categoria.Nombre) &&
                await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");
            }

            if(ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        //Mostrar la vista del formulario con los datos cargados
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }
        //Actualizar la categoria
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Categoria categoria)
        {
            if(id != categoria.Id)
            {
                return NotFound();
            }

            if(categoria.Nombre != null)
            {
                categoria.Nombre = categoria.Nombre.Trim();
            }

            if(!string.IsNullOrEmpty(categoria.Nombre) && await _context.Categorias.AnyAsync(c => c.Id != categoria.Id && c.Nombre.ToLower() == categoria.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");
            }

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoriaExistsAsync(categoria.Id))
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
            return View(categoria);
        }
        //Metodo para verificar si una categoria existe
        private async Task<bool> CategoriaExistsAsync(int id)
        {
            return await _context.Categorias.AnyAsync(e => e.Id == id);
        }

        //Mostrar la confirmacion para eliminar
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if(categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }
        //Eliminar categoria
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if(categoria == null)
            {
                return NotFound();
            }
            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(DbUpdateException e) when (e.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503")
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar esta categoría porque tiene libros asociados.");
                return View(categoria);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar la categoría. Inténtalo de nuevo.");
                return View(categoria);
            }
        }
    }
}
