using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using PeliculasAPI.DBContext;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;
using System.Runtime.CompilerServices;

namespace PeliculasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "actores";
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string contenedor = "actores"; //Nombre del contenedor donde se guardan las fotos
        public ActoresController(ApplicationDBContext context, IMapper mapper, IOutputCacheStore outputCacheStore,IAlmacenadorArchivos almacenadorArchivos)
        {
            _context = context;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet("{id:int}", Name = "ObtenerActorPorId")]
        public void Get(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ActorCreacionDTO actoreCreacionDTO) //Se usa FromForm porque viene un archivo(foto)
        { 
            var actor = _mapper.Map<Actor>(actoreCreacionDTO);

            //Si foto no es nula, se almacena
            if(actoreCreacionDTO.Foto != null)
            {
                var url = await _almacenadorArchivos.Almacenar(contenedor, actoreCreacionDTO.Foto);
                actor.Foto = url;
            }
            _context.Add(actor);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Eliminar el cache cuando se agrega un nuevo registro
            return CreatedAtRoute("ObtenerActorPorId", new { id = actor.Id }, actor);
        }
    }
}
