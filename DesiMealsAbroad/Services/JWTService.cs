using DesiMealsAbroad.DTO;
using DesiMealsAbroad.ServiceContracts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DesiMealsAbroad.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _configuration;
        public JWTService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public AuthenticationRespose createToken(ApplicationUser user)
        { 
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())

        };
            var expirationTime = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:EXPIRATION_MINUTES"]));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:KEY"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenGen = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expirationTime, // Token expiration time
                signingCredentials: credentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(tokenGen);
            var authResponse = new AuthenticationRespose
            {
                Email = user.Email,
                PersonName = user.PersonName,
                Expiration = expirationTime,
                Token = token
            };
            return authResponse;
        }
    }
}

