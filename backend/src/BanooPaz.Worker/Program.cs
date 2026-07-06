using BanooPaz.Infrastructure;
using BanooPaz.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<NotificationWorkerOptions>(
    builder.Configuration.GetSection(NotificationWorkerOptions.SectionName));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
