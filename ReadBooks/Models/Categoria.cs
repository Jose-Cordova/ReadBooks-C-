using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadBooks.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Nombre", TypeName = "varchar(50)")]
        [Required][MaxLength(50)]
        public string Nombre { get; set; }

        public virtual ICollection<Libro> Libros { get; set; }
    }
}
