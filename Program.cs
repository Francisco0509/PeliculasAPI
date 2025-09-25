using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasAPI;
using PeliculasAPI.DBContext;
using PeliculasAPI.Servicios;
using PeliculasAPI.Utilidades;
using System.Text;

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

//Configuramos Identity
builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

//Para poder crear usuarios con identity
builder.Services.AddScoped<UserManager<IdentityUser>>();

//Para poder autenticar usuarios(login)
builder.Services.AddScoped<SignInManager<IdentityUser>>();

//Agregar autenticación con JWT
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; //Para  que no mapee los claims de Microsoft a los nuestros, por que cambia los nombres de los claims
    opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)),
        ClockSkew = TimeSpan.Zero //Para que el token caduque exactamente en el tiempo que se especifica, sin tiempo de tolerancia
    };
});

//Política para que solo usuarios admin puedan crear/modificar/borrar recursos
builder.Services.AddAuthorization(opciones =>
{ 
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

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
    opciones.AddPolicy(nameof(PoliticaCacheSinAutorizacion), PoliticaCacheSinAutorizacion.Instance));
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
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();


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
