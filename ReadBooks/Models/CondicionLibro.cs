
using System.ComponentModel.DataAnnotations;
namespace ReadBooks.Models
{
    public enum CondicionLibro
    {
        [Display(Name = "Excelente")]
        Excelente,

        [Display(Name = "Bueno")]
        Bueno,

        [Display(Name = "Regular")]
        Regular,

        [Display(Name = "Dañado")]
        Danado,
    }
}
