using BanooPaz.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace BanooPaz.Worker;

public class Worker(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationWorkerOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workerOptions = options.Value;
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                var processed = await processor.ProcessPendingAsync(
                    workerOptions.BatchSize,
                    stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation("Processed {Count} notification message(s).", processed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification worker failed while processing messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, workerOptions.PollSeconds)), stoppingToken);
        }
    }
}
