using BancoInvest.Data;
using BancoInvest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BancoInvest.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        // 🔐 mesma chave do Program.cs
        private readonly string _jwtKey = "BancoInvest_Super_Secret_Key_2026_!@#_JWT_32chars_min";

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 🔹 LOGIN VIEW
        public IActionResult Login()
        {
            return View();
        }

        // 🔐 LOGIN (JWT + SESSION)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                TempData["Erro"] = "Usuário não encontrado";
                return RedirectToAction("Login");
            }

            var result = await _signInManager.PasswordSignInAsync(user, senha, false, false);

            if (!result.Succeeded)
            {
                TempData["Erro"] = "Senha inválida";
                return RedirectToAction("Login");
            }

            // 🔐 JWT TOKEN GERADO
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // 🔐 SESSION
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("JWT", jwt);

            return RedirectToAction("Index", "Conta");
        }

        // 🔓 LOGOUT
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}