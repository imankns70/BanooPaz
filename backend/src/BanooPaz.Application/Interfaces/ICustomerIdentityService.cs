using BanooPaz.Domain.Entities;

namespace BanooPaz.Application.Interfaces;

public interface ICustomerIdentityService
{
    Task<CustomerProfile> ResolveAsync(
        long? telegramUserId,
        string? telegramUsername,
        string fullName,
        string phoneNumber,
        DateTime now,
        CancellationToken cancellationToken = default);
}
