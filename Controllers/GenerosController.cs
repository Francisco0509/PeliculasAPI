using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DBContext;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerosController : ControllerBase
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "generos";
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        public GenerosController(IOutputCacheStore outputCacheStore,
                                    ApplicationDBContext context,
                                    IMapper mapper)
        {
            _outputCacheStore = outputCacheStore;
            _context = context;
            _mapper = mapper;
        }

        //Un endpoint puede tener varias rutas
        [HttpGet] // api/generos
        //[HttpGet("listado")] // api/generos/istado
        //[HttpGet("/listado-generos")] // listado-generos
        [OutputCache(Tags = [cacheTag])]
        public async Task<List<GeneroDTO>> Get([FromQuery] PaginacionDTO paginacion)
        {
            var queryable = _context.Generos;
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            return await queryable
                .OrderBy(g => g.Nombre)
                .Paginar(paginacion)
                .ProjectTo<GeneroDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id:int}", Name = "ObtenerGeneroPorId")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            var genero = await _context.Generos
                .ProjectTo<GeneroDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genero is null)
            {
                return NotFound();
            }

            return genero;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            var genero = _mapper.Map<Genero>(generoCreacionDTO);

            _context.Add(genero);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            return CreatedAtRoute("ObtenerGeneroPorId", new { id = genero.Id }, genero);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            var generoExiste = await _context.Generos.AnyAsync(g => g.Id == id);
            if (!generoExiste)
                return NotFound();

            var genero = _mapper.Map<Genero>(generoCreacionDTO);
            genero.Id = id;
            _context.Update(genero);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var registrorBorrados = await _context.Generos.Where(g => g.Id == id).ExecuteDeleteAsync();
            if(registrorBorrados == 0)
            {
                return NotFound();
            }

            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            return NoContent();
        }
    }
}
