using AssetManagement.AppService.DTOs;
using AssetManagement.Utility;

namespace AssetManagement.AppService.Contracts
{
    public interface IAuthService
    {
 Task<TokenResponse> GenerateTokenAsync(GenerateTokenRequest generateTokenRequest);
    }
}