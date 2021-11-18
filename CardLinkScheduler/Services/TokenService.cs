using CardLinkScheduler.DTOs;
using CardLinkScheduler.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CardLinkScheduler.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtInfo _jwt;
        public TokenService(IOptions<JwtInfo> jwt)
        {
            _jwt = jwt.Value;
        }
        #region generateJwtTokenOnBankId
        public string generateJwtTokenOnBankId()
        {
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _jwt.Subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("CardLinkBankId", _jwt.CardLinkBankId)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(double.Parse(_jwt.ExpiryTime));
            var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expiry, signingCredentials: signIn);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
