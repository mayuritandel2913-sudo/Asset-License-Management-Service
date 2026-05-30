using AssetManagement.Infrastructure.Entities;

namespace AssetManagement.Infrastructure.Contracts
{
    public interface IUserGenerateTokenRepository
    {
         Task<User?> GetByEmail(string email);
    }
}