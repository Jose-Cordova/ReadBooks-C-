using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReadBooks.Models;

namespace ReadBooks.Models.Context
{
    public class ReadBooksDbContext: IdentityDbContext<Usuario>
    {
        public ReadBooksDbContext(DbContextOptions<ReadBooksDbContext> options) : base(options)
        {
            
        }
        //DbSets para cada entidad
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
