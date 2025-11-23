using JwtAuthorization.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JwtAuthorization.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
