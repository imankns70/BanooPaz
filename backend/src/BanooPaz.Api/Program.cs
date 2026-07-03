using BanooPaz.Application;
using BanooPaz.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "BanooPaz.Api"
}));

app.Run();
