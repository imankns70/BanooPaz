using BanooPaz.Application.Interfaces;
using BanooPaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BanooPaz.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(BanooPazDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default) =>
        dbContext.Customers
            .Include(customer => customer.Addresses)
            .SingleOrDefaultAsync(
                customer => customer.TelegramUserId == telegramUserId,
                cancellationToken);

    public Task<Customer?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default) =>
        dbContext.Customers
            .Include(customer => customer.Addresses)
            .SingleOrDefaultAsync(
                customer => customer.PhoneNumber == phoneNumber,
                cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.AddAsync(customer, cancellationToken);
}
