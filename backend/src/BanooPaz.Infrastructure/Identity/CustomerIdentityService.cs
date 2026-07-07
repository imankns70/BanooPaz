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
        string? telegramFirstName,
        string? telegramLastName,
        string fullName,
        string phoneNumber,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var customerUsername = CreateCustomerUsername(telegramUserId, phoneNumber);
        var user = telegramUserId.HasValue
            ? await dbContext.TelegramAccounts
                .Include(account => account.User)
                .ThenInclude(candidate => candidate.CustomerProfile)!
                .ThenInclude(profile => profile!.Addresses)
                .Where(account => account.TelegramUserId == telegramUserId)
                .Select(account => account.User)
                .SingleOrDefaultAsync(cancellationToken)
            : await dbContext.Users
                .Include(candidate => candidate.TelegramAccount)
                .Include(candidate => candidate.CustomerProfile)!
                .ThenInclude(profile => profile!.Addresses)
                .OrderBy(candidate => candidate.Id)
                .FirstOrDefaultAsync(
                    candidate => candidate.PhoneNumber == phoneNumber ||
                                 candidate.UserName == customerUsername,
                    cancellationToken);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = customerUsername,
                PhoneNumber = phoneNumber,
                FullName = fullName,
                IsActive = true,
                CreatedAt = now,
                LastSeenAt = now,
                LastOrderAt = now
            };

            EnsureSucceeded(await userManager.CreateAsync(user), "create customer identity");
            EnsureSucceeded(await userManager.AddToRoleAsync(user, AppRoles.Customer), "assign Customer role");
            UpsertTelegramAccount(user, telegramUserId, telegramUsername, telegramFirstName, telegramLastName, now);

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
            UpsertTelegramAccount(user, telegramUserId, telegramUsername, telegramFirstName, telegramLastName, now);
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

    private void UpsertTelegramAccount(
        ApplicationUser user,
        long? telegramUserId,
        string? telegramUsername,
        string? telegramFirstName,
        string? telegramLastName,
        DateTime now)
    {
        if (!telegramUserId.HasValue)
        {
            return;
        }

        user.TelegramAccount ??= new TelegramAccount
        {
            User = user,
            UserId = user.Id,
            TelegramUserId = telegramUserId.Value,
            ChatId = telegramUserId.Value.ToString(),
            CreatedAt = now
        };

        user.TelegramAccount.TelegramUserId = telegramUserId.Value;
        user.TelegramAccount.Username = NormalizeOptional(telegramUsername);
        user.TelegramAccount.FirstName = NormalizeOptional(telegramFirstName);
        user.TelegramAccount.LastName = NormalizeOptional(telegramLastName);
        user.TelegramAccount.ChatId = telegramUserId.Value.ToString();
        user.TelegramAccount.LastSeenAt = now;
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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
