using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories
{
    public class UserGenerateTokenRepository: IUserGenerateTokenRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UserGenerateTokenRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<User?>GetByEmail(string email)
        {

            return await _applicationDbContext.User
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email );
        }
    }
}
