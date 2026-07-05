namespace BanooPaz.Application.Common;

public sealed record TelegramInitDataValidationResult(
    bool IsValid,
    bool IsRequired,
    string? Error,
    long? UserId,
    string? Username,
    string? FirstName,
    string? LastName)
{
    public bool CanUseDevelopmentFallback =>
        !IsRequired && string.IsNullOrWhiteSpace(Error);

    public static TelegramInitDataValidationResult Valid(
        long userId,
        string? username,
        string? firstName,
        string? lastName) =>
        new(true, true, null, userId, username, firstName, lastName);

    public static TelegramInitDataValidationResult MissingOptional() =>
        new(false, false, null, null, null, null, null);

    public static TelegramInitDataValidationResult MissingRequired() =>
        new(false, true, "Telegram initData is required.", null, null, null, null);

    public static TelegramInitDataValidationResult Invalid(
        bool isRequired,
        string error) =>
        new(false, isRequired, error, null, null, null, null);
}
