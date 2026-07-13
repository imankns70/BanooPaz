namespace Kafgir.Worker;

public sealed class NotificationWorkerOptions
{
    public const string SectionName = "NotificationWorker";

    public int PollSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 10;
}
