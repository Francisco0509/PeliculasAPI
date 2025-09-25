using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DBContext;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;
using PeliculasAPI.Utilidades;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace PeliculasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esadmin" )]
    public class ActoresController : CustomBaseController
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "actores";
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string contenedor = "actores"; //Nombre del contenedor donde se guardan las fotos
        public ActoresController(ApplicationDBContext context, IMapper mapper, IOutputCacheStore outputCacheStore,IAlmacenadorArchivos almacenadorArchivos) : base(context, mapper,outputCacheStore,cacheTag)
        {
            _context = context;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag], PolicyName = nameof(PoliticaCacheSinAutorizacion))]
        public async Task<List<ActorDTO>> Get([FromQuery] PaginacionDTO paginacion)
        {
            return await Get<Actor, ActorDTO>(paginacion, ordenarPor: a => a.Nombre);
        }


        [HttpGet("{id:int}", Name = "ObtenerActorPorId")]
        [OutputCache(Tags = [cacheTag], PolicyName = nameof(PoliticaCacheSinAutorizacion))]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            return await Get<Actor, ActorDTO>(id);
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<PeliculaActorDTO>>> Get(string nombre)
        {
            return await _context.Actores.Where(a => a.Nombre.Contains(nombre))
                .ProjectTo<PeliculaActorDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
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
            var actorDTO = _mapper.Map<ActorDTO>(actor);
            return CreatedAtRoute("ObtenerActorPorId", new { id = actor.Id }, actorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actor = await _context.Actores.FirstOrDefaultAsync(a => a.Id == id);
            if (actor is null)
            {
                return NotFound();
            }

            actor = _mapper.Map(actorCreacionDTO, actor);

            if (actorCreacionDTO.Foto is not null)
            {
                actor.Foto = await _almacenadorArchivos.Editar(actor.Foto, contenedor, actorCreacionDTO.Foto);
            }

            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await Delete<Actor>(id);
        }
    }
}
