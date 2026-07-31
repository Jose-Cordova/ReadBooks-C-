using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadBooks.Models
{
    [Table("Libros")]
    public class Libro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Titulo", TypeName = "varchar(100)")]
        [Required][MaxLength(100)]
        public string Titulo { get; set; }

        [Column("AnioPublicacion", TypeName = "integer")]
        [Required]
        public int AnioPublicacion { get; set; }

        [Column("TotalEjemplares", TypeName = "integer")]
        [Required]
        public int TotalEjemplares { get; set; }

        [Column("EjemplaresDisponibles", TypeName = "integer")]
        [Required]
        public int EjemplaresDisponibles { get; set; }

        [Column("Existe", TypeName = "boolean")]
        public bool Existe { get; set; } = true;

        [Column("AutorId", TypeName = "integer")]
        public int AutorId { get; set; }

        [Column("CategoriaId", TypeName = "integer")]
        public int CategoriaId { get; set; }

        [Column("UsuarioId", TypeName = "text")]
        public string UsuarioId { get; set; }

        //Llaves foraneas
        [ForeignKey("AutorId")]
        public virtual Autor? Autor { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<Prestamo>? Prestamos { get; set; }
    }
}
