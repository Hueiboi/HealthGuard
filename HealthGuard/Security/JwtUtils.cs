using HealthGuard.Models.Entity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthGuard.Security
{
    public interface IJwtUtils
    {
        string GenerateJwtToken(User user);
    }

    public class JwtUtils : IJwtUtils
    {
        // Chuỗi bảo mật dài đủ 256 bit (Thực tế nên để trong appsettings.json)
        private readonly string jwtSecret = "DayLaMotChuoiBaoMatCucKyDaiDeLamSecretKeyChoJWT1234567890";
        private readonly int jwtExpirationMs = 86400000; // 1 ngày

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            // Đưa thông tin Username và Role vào trong Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName.Replace("ROLE_", "")) // C# tự hiểu role nên bỏ tiền tố ROLE_ cho đẹp
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMilliseconds(jwtExpirationMs),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}