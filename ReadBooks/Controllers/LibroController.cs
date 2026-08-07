using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;
using ReadBooks.Models.Context;

namespace ReadBooks.Controllers
{
    public class LibroController : Controller
    {
        private readonly ReadBooksDbContext _context;

        public LibroController(ReadBooksDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pagina = 1)
        {
            const int tamanoPagina = 10;

            int totalRegistros = await _context.Libros.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);

            if (pagina < 1) pagina = 1;
            if (totalPaginas > 0 && pagina > totalPaginas) pagina = totalPaginas;

            var libros = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .AsNoTracking()
                .OrderBy(l => l.Titulo)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            ViewBag.Autores = await _context.Autores.AsNoTracking().ToListAsync();
            ViewBag.Categorias = await _context.Categorias.AsNoTracking().ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(libros);
        }

        public IActionResult Create()
        {
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre");
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,AnioPublicacion,TotalEjemplares,EjemplaresDisponibles,AutorId,CategoriaId")] Libro libro)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            ModelState.Remove("Autor");
            ModelState.Remove("Categoria");
            ModelState.Remove("Usuario");
            ModelState.Remove("UsuarioId");

            if (string.IsNullOrEmpty(libro.UsuarioId))
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                libro.UsuarioId = currentUserId ?? await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();
            }

            if (libro.EjemplaresDisponibles > libro.TotalEjemplares)
            {
                ModelState.AddModelError("EjemplaresDisponibles", "Los ejemplares disponibles no pueden ser mayores que el total de ejemplares.");
            }

            if (ModelState.IsValid)
            {
                bool existeTitulo = await _context.Libros.AnyAsync(l => l.Titulo.ToLower() == libro.Titulo.ToLower());
                
                if (existeTitulo)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Ya existe un libro con este título en el catálogo." });
                    }

                    ModelState.AddModelError("Titulo", "Ya existe un libro con este título.");
                    
                    ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                    ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", libro.CategoriaId);
                    return View(libro);
                }

                _context.Add(libro);
                await _context.SaveChangesAsync();

                if (isAjax)
                {
                    var autor = await _context.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == libro.AutorId);
                    var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == libro.CategoriaId);
                    return Json(new
                    {
                        success = true,
                        libro = new
                        {
                            id = libro.Id,
                            titulo = libro.Titulo,
                            anio = libro.AnioPublicacion,
                            total = libro.TotalEjemplares,
                            disponibles = libro.EjemplaresDisponibles,
                            autorId = libro.AutorId,
                            categoriaId = libro.CategoriaId,
                            autorNombre = autor?.Nombre,
                            categoriaNombre = categoria?.Nombre
                        }
                    });
                }
                
                return RedirectToAction(nameof(Index));
            }

            if (isAjax)
            {
                var primerError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Por favor verifica los datos ingresados.";
                return Json(new { success = false, message = primerError });
            }

            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }
            
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,AnioPublicacion,TotalEjemplares,EjemplaresDisponibles,AutorId,CategoriaId")] Libro libro)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id != libro.Id)
            {
                return isAjax ? Json(new { success = false, message = "Identificador de libro inválido." }) : NotFound();
            }

            ModelState.Remove("Autor");
            ModelState.Remove("Categoria");
            ModelState.Remove("Usuario");
            ModelState.Remove("UsuarioId");

            if (libro.EjemplaresDisponibles > libro.TotalEjemplares)
            {
                ModelState.AddModelError("EjemplaresDisponibles", "Los ejemplares disponibles no pueden ser mayores que el total de ejemplares.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool existeTitulo = await _context.Libros.AnyAsync(l => l.Id != libro.Id && l.Titulo.ToLower() == libro.Titulo.ToLower());
                    if (existeTitulo)
                    {
                        if (isAjax)
                        {
                            return Json(new { success = false, message = "Ya existe otro libro con este título en el catálogo." });
                        }

                        ModelState.AddModelError("Titulo", "Ya existe un libro con este título.");
                        ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", libro.CategoriaId);
                        return View(libro);
                    }

                    var libroOriginal = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
                    if (libroOriginal != null)
                    {
                        libro.UsuarioId = libroOriginal.UsuarioId;
                    }

                    _context.Update(libro);
                    await _context.SaveChangesAsync();

                    if (isAjax)
                    {
                        var autor = await _context.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == libro.AutorId);
                        var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == libro.CategoriaId);
                        return Json(new
                        {
                            success = true,
                            libro = new
                            {
                                id = libro.Id,
                                titulo = libro.Titulo,
                                anio = libro.AnioPublicacion,
                                total = libro.TotalEjemplares,
                                disponibles = libro.EjemplaresDisponibles,
                                autorId = libro.AutorId,
                                categoriaId = libro.CategoriaId,
                                autorNombre = autor?.Nombre,
                                categoriaNombre = categoria?.Nombre
                            }
                        });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibroExists(libro.Id))
                    {
                        return isAjax ? Json(new { success = false, message = "El libro ya no existe." }) : NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }

            if (isAjax)
            {
                var primerError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Por favor verifica los datos ingresados.";
                return Json(new { success = false, message = primerError });
            }
            
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", libro.CategoriaId);
            return View(libro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro != null)
            {
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCategoriaAjax([FromBody] Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria?.Nombre))
                return Json(new { success = false, message = "El nombre no puede estar vacío." });

            categoria.Nombre = categoria.Nombre.Trim();

            bool existe = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());
            if (existe)
                return Json(new { success = false, message = "Ya existe una categoría con ese nombre." });

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = categoria.Id, nombre = categoria.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> EditarCategoriaAjax([FromBody] Categoria categoriaActualizada)
        {
            var categoria = await _context.Categorias.FindAsync(categoriaActualizada.Id);
            if (categoria == null)
                return Json(new { success = false, message = "Categoría no encontrada." });

            categoriaActualizada.Nombre = categoriaActualizada.Nombre?.Trim() ?? "";
            bool existe = await _context.Categorias.AnyAsync(c => c.Id != categoriaActualizada.Id && c.Nombre.ToLower() == categoriaActualizada.Nombre.ToLower());
            if (existe)
                return Json(new { success = false, message = "Ya existe una categoría con ese nombre." });

            categoria.Nombre = categoriaActualizada.Nombre;
            await _context.SaveChangesAsync();

            return Json(new { success = true, nombre = categoria.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCategoriaAjax([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                int id = body.GetProperty("id").GetInt32();
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                    return Json(new { success = false, message = "Categoría no encontrada." });

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException e) when (e.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503")
            {
                return Json(new { success = false, message = "No se puede eliminar: tiene libros asociados." });
            }
            catch
            {
                return Json(new { success = false, message = "Ocurrió un error inesperado." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarAutorAjax([FromBody] Autor autor)
        {
            if (string.IsNullOrWhiteSpace(autor?.Nombre))
                return Json(new { success = false, message = "El nombre no puede estar vacío." });

            autor.Nombre = autor.Nombre.Trim();

            bool existe = await _context.Autores.AnyAsync(a => a.Nombre.ToLower() == autor.Nombre.ToLower());
            if (existe)
                return Json(new { success = false, message = "Ya existe un autor con ese nombre." });

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = autor.Id, nombre = autor.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> EditarAutorAjax([FromBody] Autor autorActualizado)
        {
            var autor = await _context.Autores.FindAsync(autorActualizado.Id);
            if (autor == null)
                return Json(new { success = false, message = "Autor no encontrado." });

            autorActualizado.Nombre = autorActualizado.Nombre?.Trim() ?? "";
            bool existe = await _context.Autores.AnyAsync(a => a.Id != autorActualizado.Id && a.Nombre.ToLower() == autorActualizado.Nombre.ToLower());
            if (existe)
                return Json(new { success = false, message = "Ya existe un autor con ese nombre." });

            autor.Nombre = autorActualizado.Nombre;
            await _context.SaveChangesAsync();

            return Json(new { success = true, nombre = autor.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarAutorAjax([FromBody] System.Text.Json.JsonElement body)
        {
            try
            {
                int id = body.GetProperty("id").GetInt32();
                var autor = await _context.Autores.FindAsync(id);
                if (autor == null)
                    return Json(new { success = false, message = "Autor no encontrado." });

                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException e) when (e.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503")
            {
                return Json(new { success = false, message = "No se puede eliminar: tiene libros asociados." });
            }
            catch
            {
                return Json(new { success = false, message = "Ocurrió un error inesperado." });
            }
        }
    }
}
