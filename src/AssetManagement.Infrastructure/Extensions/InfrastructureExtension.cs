using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Repositories;
using AssetManagement.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AssetManagement.Infrastructure.Extensions;

public static class InfrastructureExtension
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : AddInfrastructure
    /// Description   : Register infrastructure related services to DI container
    /// Creation-Date : 23rd March 2026
    /// </summary>
    /// <param name="infrastructure"></param>
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
         services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection")));
            services.AddScoped<IUserGenerateTokenRepository, UserGenerateTokenRepository>();
            services.AddScoped<IMasterServiceRepository, MasterServiceRepository>();
               services.AddScoped<IAssetRepository,AssetRepository>();
         
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ILicenseRepository, LicenseRepository>();

           
            services.AddScoped<INotificationRepository, NotificationRepository>();
    }
}
