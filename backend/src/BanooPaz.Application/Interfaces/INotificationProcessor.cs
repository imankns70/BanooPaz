namespace BanooPaz.Application.Interfaces;

public interface INotificationProcessor
{
    Task<int> ProcessPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}
