using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.Infrastructure.Contracts;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using AssetManagement.Utility.Resource;
using AssetManagement.Utility;
using AssetManagement.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace AssetManagement.AppService.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserGenerateTokenRepository _userGenerateTokenRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IOptions<JwtSettings> jwtSettings, IUserGenerateTokenRepository repo, ILogger<AuthService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _userGenerateTokenRepository = repo;
            _logger = logger;
        }

        public async Task<TokenResponse> GenerateTokenAsync(GenerateTokenRequest generateTokenRequest)
        {
            var user = await _userGenerateTokenRepository.GetByEmail(generateTokenRequest.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found.", generateTokenRequest.Email);
                throw new UnAuthServiceorizedException(CommonResource.UnAuthServiceorized);
            }

            
            if (user.IsActive != true)
            {
                _logger.LogWarning("Login failed: User {Email} is inactive.", generateTokenRequest.Email);
                throw new UnAuthServiceorizedException(CommonResource.UserInactive);
            }

         
            if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer) || string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            {
                _logger.LogError("Token generation failed: Configuration missing.");
                throw new InternalServerException("Token generation configuration is missing.");
            }

            _logger.LogInformation("User {Email} authenticated. Generating token.", generateTokenRequest.Email);
            
            var tokenData = BuildToken(user.UserID.ToString(), generateTokenRequest.Email, user.Role?.RoleName ?? string.Empty);
            
            return tokenData;
        }

        private TokenResponse BuildToken(string userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);
            var istZone = TimeZoneInfo.FindSystemTimeZoneById(CommonResource.IndiaTimeZone);
            var expiryIst = TimeZoneInfo.ConvertTimeFromUtc(expiryUtc, istZone);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiryUtc,
                signingCredentials: creds
            );

            return new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiryIn = expiryIst.ToString("hh:mm:ss tt")
            };
        }
    }
}
