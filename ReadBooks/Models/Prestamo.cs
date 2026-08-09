using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadBooks.Models
{
    [Table("Prestamos")]
    public class Prestamo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Fecha", TypeName = "timestamp")]
        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Column("FechaDevolucionProgramada", TypeName = "timestamp")]
        [Required]
        public DateTime FechaDevolucionProgramada { get; set; }

        [Column("CondicionAlEntregar", TypeName = "varchar(255)")]
        [Required]
        public string CondicionAlEntregar { get; set; }

        [Column("CondicionAlDevolver", TypeName = "varchar(255)")]
        [Required]
        public string CondicionAlDevolver { get; set; }

        [Column("Cantidad", TypeName = "integer")]
        [Required]
        public int Cantidad { get; set; }

        [Column("Estado", TypeName = "varchar(20)")]
        [Required]
        public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.PRESTADO;

        [Column("LibroId", TypeName = "integer")]
        public int LibroId { get; set; }

        [Column("EstudianteId", TypeName = "integer")]
        public int EstudianteId { get; set; }

        //Llaves foraneas
        [ForeignKey("LibroId")]
        public virtual Libro? Libro { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Estudiante? Estudiante { get; set; }
    }
}
