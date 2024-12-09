using System.Diagnostics;
using System.Security.Claims;
using glamping_addventure3.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace glamping_addventure3.Controllers
{
    [Authorize]
    [Authorize]
    [Authorize(Policy = "AccederDashboard")]
    public class HomeController : Controller
    {
        private GlampingAddventure3Context _dbcontext;

        public HomeController(GlampingAddventure3Context context)
        {
            _dbcontext = context;
        }
        public IActionResult Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            string nombreUsuario = "";

            if (claimuser.Identity.IsAuthenticated)
            {
                nombreUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.Name).
                        Select(c => c.Value).SingleOrDefault();
            }

            ViewData["nombreUsuario"] = nombreUsuario;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("IniciarSesion", "Inicio");
        }
    }
}
