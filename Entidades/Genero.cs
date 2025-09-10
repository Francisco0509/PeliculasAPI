using PeliculasAPI.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace PeliculasAPI.Entidades
{
    public class Genero
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener {1} caractreres o menos.")]
        [PrimeraLetraMayuscula] //validacion personalizada
        public required string Nombre { get; set; }
    }
}
