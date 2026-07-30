using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ReadBooks.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ReadBooks.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        public AccountController(SignInManager<Usuario> singInManager, UserManager<Usuario> userManager)
        {
            _signInManager = singInManager;
            _userManager = userManager;
        }

        //Mostramos el formulario de inicio de sesion
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //Prosesamos las credenciales ingresadas
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //Buscamos a ver si existe ese usuario
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    //Con PasswordSignInAsync validamoa la contraseña encriptada
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email,
                        model.Password,
                        false,
                        false
                    );

                    if (result.Succeeded)
                    {
                        //Redijirimos al usuario al inicio
                        if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            }
            return View(model);
        }

        //Metodo para cerrar sesionpartial
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
