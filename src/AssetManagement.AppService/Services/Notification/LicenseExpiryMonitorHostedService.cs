using AssetManagement.AppService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services;


public class LicenseExpiryMonitorHostedService : BackgroundService
{
    private readonly ILogger<LicenseExpiryMonitorHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;
    private readonly TimeSpan timeSpan;

    public LicenseExpiryMonitorHostedService(
        ILogger<LicenseExpiryMonitorHostedService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var intervalHours = configuration.GetValue<int?>("LicenseExpiryMonitorHostedService:IntervalHours") ?? 24;
        timeSpan = TimeSpan.FromHours(intervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("License Expiry Monitor Hosted Service is starting");

        await DoWorkAsync();

        _timer = new Timer(
            async _ => await DoWorkAsync(),
            null,
            timeSpan,
            timeSpan);
        await Task.CompletedTask;
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var licenseExpiryMonitorService = scope.ServiceProvider.GetRequiredService<ILicenseExpiryMonitorService>();
                await licenseExpiryMonitorService.ExecuteAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in License Expiry Monitor Hosted Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("License Expiry Monitor Hosted Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
