using PeliculasAPI.Entidades;
using PeliculasAPI.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeliculasAPI.DTOs
{
    public class GeneroDTO : IId
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
    }
}
