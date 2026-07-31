using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadBooks.Models
{
    [Table("Estudiantes")]
    public class Estudiante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Nombre", TypeName = "varchar(50)")]
        [Required][MaxLength(50)]
        public string Nombre { get; set; }

        [Column("Apellido", TypeName = "varchar(50)")]
        [Required][MaxLength(50)]
        public string Apellido { get; set; }

        [Column("Carnet", TypeName = "varchar(10)")]
        [Required][MaxLength(10)]
        public string Carnet { get; set; }

        [Column("Telefono", TypeName = "varchar(8)")]
        [MaxLength(8)]
        public string? Telefono { get; set; }

        public virtual ICollection<Prestamo>? Prestamos { get; set; }
    }
}
