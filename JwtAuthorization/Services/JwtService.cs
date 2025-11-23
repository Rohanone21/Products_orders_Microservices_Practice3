using JwtAuthorization.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthorization.Services
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration config)
        {
            _key = config["Jwt:Key"];
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];
        }

        public string GenerateToken(User user)
        {
            if (string.IsNullOrEmpty(_key))
                throw new Exception("JWT Key is missing from configuration.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
