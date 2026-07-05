using BanooPaz.Contracts.Customers;

namespace BanooPaz.Application.Interfaces;

public interface ICustomerProfileService
{
    Task<CustomerProfileDto?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default);
}
