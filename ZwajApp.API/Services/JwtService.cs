using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ZwajApp.API.Models;

namespace ZwajApp.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(User user)
        {
            // 1. تحديد البيانات التي ستوضع داخل التوكن (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            // 2. جلب المفتاح السري من إعدادات المشروع (appsettings.json)
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            // 3. تحديد نوع التشفير
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // 4. بناء التوكن
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(12), // صلاحية التوكن 12 ساعة
                SigningCredentials = creds
            };

            // 5. هذا الذي يأخذ ال tokenDescriptor ويبني من خلاله التوكن
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 6. 
            return tokenHandler.WriteToken(token);
        }
    }
}