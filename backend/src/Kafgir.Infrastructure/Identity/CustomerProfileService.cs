using Kafgir.Application.Interfaces;
using Kafgir.Contracts.Customers;
using Kafgir.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kafgir.Infrastructure.Identity;

public sealed class CustomerProfileService(KafgirDbContext dbContext) : ICustomerProfileService
{
    public async Task<CustomerProfileDto?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.TelegramAccounts
            .AsNoTracking()
            .Include(account => account.User)
            .ThenInclude(candidate => candidate.CustomerProfile)!
            .ThenInclude(profile => profile!.Addresses)
            .Where(account => account.TelegramUserId == telegramUserId)
            .Select(account => account.User)
            .SingleOrDefaultAsync(cancellationToken);
        var profile = user?.CustomerProfile;
        if (profile is null)
        {
            return null;
        }

        return new CustomerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            PreferredName = profile.PreferredName,
            DefaultPhoneNumber = profile.DefaultPhoneNumber,
            Addresses = profile.Addresses
                .Where(address => address.IsActive)
                .OrderByDescending(address => address.IsDefault)
                .Select(address => new CustomerAddressDto
                {
                    Id = address.Id,
                    Title = address.Title,
                    City = address.City,
                    AddressLine = address.AddressLine,
                    Description = address.Description,
                    IsDefault = address.IsDefault
                })
                .ToList()
        };
    }
}
