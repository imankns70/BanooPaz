using BanooPaz.Application.Interfaces;
using BanooPaz.Contracts.Customers;
using BanooPaz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Identity;

public sealed class CustomerProfileService(BanooPazDbContext dbContext) : ICustomerProfileService
{
    public async Task<CustomerProfileDto?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(candidate => candidate.CustomerProfile)!
            .ThenInclude(profile => profile!.Addresses)
            .SingleOrDefaultAsync(candidate => candidate.TelegramUserId == telegramUserId, cancellationToken);
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
