using Kafgir.Domain.Entities;

namespace Kafgir.Application.Interfaces;

public interface ICustomerIdentityService
{
    Task<CustomerProfile> ResolveAsync(
        long? telegramUserId,
        string? telegramUsername,
        string? telegramFirstName,
        string? telegramLastName,
        string fullName,
        string phoneNumber,
        DateTime now,
        CancellationToken cancellationToken = default);
}
