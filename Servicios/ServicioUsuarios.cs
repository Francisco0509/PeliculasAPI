using Microsoft.AspNetCore.Identity;

namespace PeliculasAPI.Servicios
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<string> ObtenerUsuarioId()
        {
            var email = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "email")!.Value;
            var usuario = await _userManager.FindByEmailAsync(email);
            return usuario!.Id;
        }
    }
}
