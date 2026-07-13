using Kafgir.Application.Common;

namespace Kafgir.Application.Interfaces;

public interface ITelegramInitDataValidator
{
    TelegramInitDataValidationResult Validate(string? initData);
}
