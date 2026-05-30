using AssetManagement.AppService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services;

public class LicenseRenewalHostedService : BackgroundService
{
    private readonly ILogger<LicenseRenewalHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _timeSpan;

    public LicenseRenewalHostedService(
        ILogger<LicenseRenewalHostedService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var intervalHours = configuration.GetValue<int?>("LicenseRenewalHostedService:IntervalHours");
        if (!intervalHours.HasValue || intervalHours.Value <= 0)
        {
            throw new InvalidOperationException("LicenseRenewalHostedService:IntervalHours must be a positive integer.");
        }

        _timeSpan = TimeSpan.FromHours(intervalHours.Value);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("License Renewal Hosted Service is starting");

        await DoWorkAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("License Renewal Hosted Service next run scheduled after {Delay}", _timeSpan);

            try
            {
                await Task.Delay(_timeSpan, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await DoWorkAsync();
        }
    }

    private  async Task DoWorkAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var licenseRenewalService = scope.ServiceProvider.GetRequiredService<ILicenseRenewalService>();
            await licenseRenewalService.ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in License Renewal Hosted Service");
        }
    }
}
