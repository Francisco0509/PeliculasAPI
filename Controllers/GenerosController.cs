using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esadmin")]
    public class GenerosController : CustomBaseController
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cacheTag = "generos";
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public GenerosController(IOutputCacheStore outputCacheStore,
                                    ApplicationDBContext context,
                                    IMapper mapper) : base(context, mapper,outputCacheStore,cacheTag)
        {
            _outputCacheStore = outputCacheStore;
            _context = context;
            _mapper = mapper;
        }

        //Un endpoint puede tener varias rutas
        [HttpGet] // api/generos
        //[HttpGet("listado")] // api/generos/istado
        //[HttpGet("/listado-generos")] // listado-generos
        [OutputCache(Tags = [cacheTag], PolicyName = nameof(PoliticaCacheSinAutorizacion))]
        public async Task<List<GeneroDTO>> Get([FromQuery] PaginacionDTO paginacion)
        {
            return await Get<Genero, GeneroDTO>(paginacion, ordenarPor: g => g.Nombre);
        }

        [HttpGet("todos")]
        [OutputCache(Tags = [cacheTag], PolicyName = nameof(PoliticaCacheSinAutorizacion))]
        [AllowAnonymous]
        public async Task<List<GeneroDTO>> Get()
        { 
            return await Get<Genero, GeneroDTO>(ordenarPor: g  => g.Nombre);
        }


        [HttpGet("{id:int}", Name = "ObtenerGeneroPorId")]
        [OutputCache(Tags = [cacheTag], PolicyName = nameof(PoliticaCacheSinAutorizacion))]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            return await Get<Genero, GeneroDTO>(id);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            return await Post<GeneroCreacionDTO, Genero, GeneroDTO>(generoCreacionDTO, "ObtenerGeneroPorId");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            return await Put<GeneroCreacionDTO, Genero>(id, generoCreacionDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await Delete<Genero>(id);   
        }
    }
}
