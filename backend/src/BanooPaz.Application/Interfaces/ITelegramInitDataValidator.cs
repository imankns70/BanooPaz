using BanooPaz.Application.Common;

namespace BanooPaz.Application.Interfaces;

public interface ITelegramInitDataValidator
{
    TelegramInitDataValidationResult Validate(string? initData);
}
