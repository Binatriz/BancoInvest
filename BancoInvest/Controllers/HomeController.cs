using Microsoft.AspNetCore.Mvc;

namespace BancoInvest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}