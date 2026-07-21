using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadBooks.Models
{
    [Table("Usuarios")]
    public class Usuario : IdentityUser
    {
        public virtual ICollection<Libro> Libros { get; set; }
    }
}
