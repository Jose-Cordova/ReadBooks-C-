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
        [Required(ErrorMessage = "El Nombre es obligatorio. ")]
        [MaxLength(50)]

        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras.")]
        public string Nombre { get; set; }

        [Column("Apellido", TypeName = "varchar(50)")]
        [Required(ErrorMessage = "El  Apellido es obligatorio.")]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras.")]
        public string Apellido { get; set; }

        [Column("Carnet", TypeName = "varchar(10)")]
        [Required(ErrorMessage = "El Carnet es obligatorio.")]
        [MaxLength(10)]
        public string Carnet { get; set; }

        [Column("Telefono", TypeName = "varchar(8)")]
        [MaxLength(8)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono solo puede contener números.")]
        public string? Telefono { get; set; }

        public virtual ICollection<Prestamo>? Prestamos { get; set; }
    }
}
