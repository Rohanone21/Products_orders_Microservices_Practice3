using JwtAuthorization.Data;
using JwtAuthorization.Models;
using JwtAuthorization.Services;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthorization.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;
        private readonly AuthDbContext _context;

        public AccountController(UserService userService, JwtService jwtService, AuthDbContext context)
        {
            _userService = userService;
            _jwtService = jwtService;
            _context = context;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult ForgotPassword() => View();

        // Login POST
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var user = _userService.ValidateUser(model.Email, model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Credentials!";
                return View();
            }

            string token = _jwtService.GenerateToken(user);

            HttpContext.Response.Cookies.Append("jwt", token);

            return RedirectToAction("Index", "Home");
        }

        // Register POST
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (_userService.EmailExists(model.Email))
            {
                ViewBag.Error = "Email already exists!";
                return View();
            }

            _userService.Register(model);

            return RedirectToAction("Login");
        }

        // Forgot Password POST
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user != null)
            {
                user.ResetToken = Guid.NewGuid().ToString();
                user.ResetTokenExpiry = DateTime.Now.AddHours(1);

                _userService.SaveResetToken(user);
            }

            ViewBag.Message = "If this email exists, reset link is generated.";
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }
    }
}
