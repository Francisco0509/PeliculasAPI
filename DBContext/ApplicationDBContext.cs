using Microsoft.EntityFrameworkCore;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.DBContext
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {                
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PeliculaActor>()
                .HasKey(pa => new { pa.PeliculaId, pa.ActorId });
            modelBuilder.Entity<PeliculaCine>()
                .HasKey(pa => new { pa.PeliculaId, pa.CineId });
            modelBuilder.Entity<PeliculaGenero>()
                .HasKey(pa => new { pa.PeliculaId, pa.GeneroId });
        }

        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Cine> Cines { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<PeliculaActor> PeliculasActores { get; set; }
        public DbSet<PeliculaCine> PeliculasCines { get; set; }
        public DbSet<PeliculaGenero> PeliculasGeneros { get; set; }
    }
}
