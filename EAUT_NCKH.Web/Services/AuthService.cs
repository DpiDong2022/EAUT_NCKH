using EAUT_NCKH.Web.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EAUT_NCKH.Web.Services {
    public class AuthService {

        private readonly IConfiguration _config;
        public AuthService(IConfiguration configuration) {

            _config = configuration;
        }

        public string GenerateJwtToken(int accountId, string roleName, int minutes) {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };

            var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials
        );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token) {

            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
                return false;

            var jwt = tokenHandler.ReadJwtToken(token);

            return jwt.ValidTo > DateTime.UtcNow;
        }

        public int? GetAccountIdFromToken(string token) {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
                return null;

            var jwtToken = tokenHandler.ReadJwtToken(token);

            var accountIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (accountIdClaim == null)
                return null;

            if (int.TryParse(accountIdClaim.Value, out int accountId))
                return accountId;

            return null;
        }

        public string GetRoleFromToken(string token) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }
    }
}
