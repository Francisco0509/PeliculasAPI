﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class PeliculaCreacionDTO
    {
        [Required]
        [StringLength(300, ErrorMessage = "El campo {0} debe ser menor a {1} caracteres")]
        public required string Titulo { get; set; }
        public string? Trailer { get; set; }
        public DateTime FechaLanzamiento { get; set; }
        [Unicode(false)]
        public IFormFile? Poster { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder))]
        public List<int>? GenerosIds { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder))]
        public List<int>? CinesIds { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder))]
        public List<ActorPeliculaCreacionDTO>? Actores { get; set; }

    }
}
