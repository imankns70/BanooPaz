using Kafgir.Contracts.Customers;

namespace Kafgir.Application.Interfaces;

public interface ICustomerProfileService
{
    Task<CustomerProfileDto?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default);
}
