using AssetManagement.AppService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AssetManagement.AppService.Services;


public class EmailNotificationHostedService : BackgroundService
{
    private readonly ILogger<EmailNotificationHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;
    private readonly TimeSpan _timeSpan;
    private readonly int _delaySeconds;

    public EmailNotificationHostedService(
        ILogger<EmailNotificationHostedService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var intervalMinutes = configuration.GetValue<int?>("EmailNotificationHostedService:IntervalMinutes") ?? 5;
        _timeSpan = TimeSpan.FromMinutes(intervalMinutes);
        _delaySeconds = configuration.GetValue<int?>("EmailNotificationHostedService:InitialDelaySeconds") ?? 30;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Notification Hosted Service is starting");

      
        _timer = new Timer(
            async _ => await DoWorkAsync(),
            null,
            TimeSpan.FromSeconds(_delaySeconds),
            _timeSpan);

        await Task.CompletedTask;
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var emailNotificationService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
                await emailNotificationService.ExecuteAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Email Notification Hosted Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Notification Hosted Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
