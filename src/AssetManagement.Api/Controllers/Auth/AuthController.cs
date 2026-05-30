using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Generatetoken")]
        public async Task<IActionResult> GenerateToken(GenerateTokenRequest dto)
        {
            _logger.LogInformation("Login request received for email {Email}.", dto.Email);

            var tokenResponse = await _authService.GenerateTokenAsync(dto);

            return Ok(tokenResponse, CommonResource.TokenGenerated);
        }
    }
}