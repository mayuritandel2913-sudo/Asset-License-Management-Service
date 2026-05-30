using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.Services;
using AssetManagement.AppService.Services.AdminService;
using AssetManagement.AppService.Services.Notification;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetManagement.AppService.Extensions;

public static class ServiceExtension
{
  
    public static void AddService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMasterService, MasterService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<ILicenseRenewalService, LicenseRenewalService>();

      
      
        services.AddScoped<ILicenseExpiryMonitorService, LicenseExpiryMonitorService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        services.AddHostedService<LicenseRenewalHostedService>();
        services.AddHostedService<LicenseExpiryMonitorHostedService>();
        services.AddHostedService<EmailNotificationHostedService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
    }
}
