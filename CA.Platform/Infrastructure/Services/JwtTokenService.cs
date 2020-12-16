using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities.Identity;
using CA.Platform.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CA.Platform.Infrastructure.Services
{
    public class JwtTokenService: ITokenService
    {
        private readonly AppSettings _appSettings;
        
        public JwtTokenService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        
        public string GetToken(User user, List<Permission> permissions)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new [] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim("User", JsonSerializer.Serialize(new UserDto(user))),
                    new Claim("Permissions", JsonSerializer.Serialize(permissions.Select(a => new PermissionDto(a)))),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}