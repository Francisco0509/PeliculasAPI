using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using PeliculasAPI.DBContext;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeliculasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomBaseController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly string cacheTag;
        public CustomBaseController(ApplicationDBContext context, IMapper mapper, IOutputCacheStore outputCacheStore, string cacheTag)
        {
            _context = context;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
            this.cacheTag = cacheTag;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(Expression<Func<TEntidad, object>> ordenarPor) where TEntidad : class
        {
            return await _context.Set<TEntidad>()
                .OrderBy(ordenarPor)
                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacion, Expression<Func<TEntidad, object>> ordenarPor) where TEntidad : class
        { 
            var queryable = _context.Set<TEntidad>().AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable); //Insertar en la cabecera la cantidad total de registros

            return await queryable
                .OrderBy(ordenarPor)
                .Paginar(paginacion)
                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        protected async Task<ActionResult<TDTO>> Get<TEntidad, TDTO>(int id) 
            where TEntidad : class, IId
            where TDTO : IId
        {
            var entidad = await _context.Set<TEntidad>()
                .ProjectTo<TDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entidad is null)
            {
                return NotFound();
            }

            return entidad;
        }

        protected async Task<IActionResult> Post<TCreacionDTO, TEntidad, TDTO>(TCreacionDTO creacionDTO, string nombreRuta) 
            where TEntidad : class, IId
            where TDTO : IId
        {
            var entidad = _mapper.Map<TEntidad>(creacionDTO);
            _context.Add(entidad);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            var entidadDTO = _mapper.Map<TDTO>(entidad);
            return CreatedAtRoute(nombreRuta, new { id = entidad.Id }, entidadDTO);
        }

        protected async Task<IActionResult> Put<TCreacionDTO, TEntidad>(int id, TCreacionDTO creacionDTO)
            where TEntidad : class, IId
        {
            var entidadExiste = await _context.Set<TEntidad>().AnyAsync(g => g.Id == id);
            if (!entidadExiste)
                return NotFound();

            var entidad = _mapper.Map<TEntidad>(creacionDTO);
            entidad.Id = id;
            _context.Update(entidad);
            await _context.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            return NoContent();
        }

        protected async Task<IActionResult> Delete<TEntidad>(int id)
            where TEntidad : class, IId
        {
            var registrorBorrados = await _context.Set<TEntidad>().Where(g => g.Id == id).ExecuteDeleteAsync();
            if (registrorBorrados == 0)
            {
                return NotFound();
            }

            await _outputCacheStore.EvictByTagAsync(cacheTag, default); //Limpiar cache
            return NoContent();
        }
    }
}
