using Microsoft.IdentityModel.Tokens;
using SecureTokenAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureTokenAPI.Services
    {
    public class JwtService : IJwtService
        {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
            {
            _configuration = configuration;
            }

        public string GenerateJwtToken(string username, List<string> roles, Guid userToken)
            {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserToken", userToken.ToString())
            };

           
            if (!roles.Contains("user"))
                {
                roles.Add("user");  
                }

            roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(double.Parse(jwtSettings["ExpiresInHours"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }