using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class Cine : IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(75, ErrorMessage = "El campo {0} no puede ser mayor a {1} caracteres")]
        public required string Nombre { get; set; }
        public required Point Ubicacion { get; set; }
    }
}
