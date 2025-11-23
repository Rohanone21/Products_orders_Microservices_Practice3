using JwtAuthorization.Data;
using JwtAuthorization.Models;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthorization.Services
{
    public class UserService
    {
        private readonly AuthDbContext _context;

        public UserService(AuthDbContext context)
        {
            _context = context;
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public User Register(RegisterViewModel model)
        {
            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public User ValidateUser(string email, string password)
        {
            string hash = HashPassword(password);
            return _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);
        }

        public void SaveResetToken(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
