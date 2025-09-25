using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DBContext;
using PeliculasAPI.DTOs;
using PeliculasAPI.Servicios;

namespace PeliculasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IServicioUsuarios _servicioUsuarios;

        public RatingsController(ApplicationDBContext context, IServicioUsuarios servicioUsuarios)
        {
            _context = context;
            _servicioUsuarios = servicioUsuarios;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] RatingCreacionDTO ratingCreacionDTO) 
        {
            var usuario = await _servicioUsuarios.ObtenerUsuarioId();

            var ratingActual = await _context.RatingsPeliculas
                .FirstOrDefaultAsync(x => x.PeliculaId == ratingCreacionDTO.PeliculaId && x.UsuarioId == usuario);

            if (ratingActual == null)
            {
                var rating = new Entidades.Rating
                {
                    PeliculaId = ratingCreacionDTO.PeliculaId,
                    Puntuacion = ratingCreacionDTO.Puntuacion,
                    UsuarioId = usuario
                };

                _context.Add(rating);
            }
            else
            { 
                ratingActual.Puntuacion = ratingCreacionDTO.Puntuacion;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
