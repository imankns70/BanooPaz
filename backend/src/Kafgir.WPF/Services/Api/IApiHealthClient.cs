namespace Kafgir.WPF.Services.Api;

public interface IApiHealthClient
{
    Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken = default);
}
