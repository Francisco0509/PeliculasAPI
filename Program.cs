using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasAPI;
using PeliculasAPI.DBContext;
using PeliculasAPI.Servicios;
using PeliculasAPI.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Configurar AutoMapper
builder.Services.AddSingleton(proveedor => new MapperConfiguration(config =>
{
    var geometryFactory = proveedor.GetRequiredService<GeometryFactory>();
    config.AddProfile(new AutoMapperProfiles(geometryFactory));
}).CreateMapper());
//builder.Services.AddAutoMapper(typeof(Program)); revisar por que no funciona esta línea

//Configuramos el Db Context
builder.Services.AddDbContext<ApplicationDBContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServer => sqlServer.UseNetTopologySuite());
});

//Configurar uso de coordenadas del planeta tierra
builder.Services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

//Activar cache, para guardar consultas lentas y poder accesar los datos más rapidamente
builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60); //El tiempo en que va a estar activo el cache
});

//Habilitar CORS
var origenesPermitidos = builder.Configuration.GetValue<string>("OrigenesPermitidos")!.Split(",");
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
            .WithExposedHeaders("cantidad-total-registros");
        //opcionesCORS.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); //Cualquier origen, cualquier método(Get,Post, Put, Delete), cualquier cabecera
    });
});

//Servicio para almacenar archivos
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
builder.Services.AddHttpContextAccessor(); //Para obtener la URL del servidor


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); //Para poder servir archivos estáticos como las imágenes

app.UseCors();

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.Run();
