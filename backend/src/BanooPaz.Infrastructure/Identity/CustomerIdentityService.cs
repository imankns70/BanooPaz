using BanooPaz.Application.Common;
using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using BanooPaz.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Identity;

public sealed class CustomerIdentityService(
    BanooPazDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ICustomerIdentityService
{
    public async Task<CustomerProfile> ResolveAsync(
        long? telegramUserId,
        string? telegramUsername,
        string fullName,
        string phoneNumber,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var user = telegramUserId.HasValue
            ? await dbContext.Users
                .Include(candidate => candidate.CustomerProfile)!
                .ThenInclude(profile => profile!.Addresses)
                .SingleOrDefaultAsync(candidate => candidate.TelegramUserId == telegramUserId, cancellationToken)
            : await dbContext.Users
                .Include(candidate => candidate.CustomerProfile)!
                .ThenInclude(profile => profile!.Addresses)
                .OrderBy(candidate => candidate.Id)
                .FirstOrDefaultAsync(candidate => candidate.PhoneNumber == phoneNumber, cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = CreateCustomerUsername(telegramUserId, phoneNumber),
                TelegramUserId = telegramUserId,
                PhoneNumber = phoneNumber,
                FullName = fullName,
                IsActive = true,
                CreatedAt = now,
                LastSeenAt = now,
                LastOrderAt = now
            };

            EnsureSucceeded(await userManager.CreateAsync(user), "create customer identity");
            EnsureSucceeded(await userManager.AddToRoleAsync(user, AppRoles.Customer), "assign Customer role");

            user.CustomerProfile = new CustomerProfile
            {
                UserId = user.Id,
                PreferredName = fullName,
                DefaultPhoneNumber = phoneNumber,
                CreatedAt = now,
                LastOrderAt = now
            };
            dbContext.CustomerProfiles.Add(user.CustomerProfile);
        }
        else
        {
            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.LastSeenAt = now;
            user.LastOrderAt = now;
            EnsureSucceeded(await userManager.UpdateAsync(user), "update customer identity");

            if (user.CustomerProfile is null)
            {
                user.CustomerProfile = new CustomerProfile
                {
                    UserId = user.Id,
                    CreatedAt = now
                };
                dbContext.CustomerProfiles.Add(user.CustomerProfile);
            }

            user.CustomerProfile.PreferredName = fullName;
            user.CustomerProfile.DefaultPhoneNumber = phoneNumber;
            user.CustomerProfile.LastOrderAt = now;
        }

        return user.CustomerProfile;
    }

    private static string CreateCustomerUsername(long? telegramUserId, string phoneNumber)
    {
        if (telegramUserId.HasValue)
        {
            return $"tg_{telegramUserId.Value}";
        }

        var safePhone = new string(phoneNumber.Where(char.IsLetterOrDigit).ToArray());
        return safePhone.Length == 0 ? $"phone_{Guid.NewGuid():N}" : $"phone_{safePhone}";
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
