namespace Shared.Contracts.Events;

public record ForgotPasswordEvent
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Otp { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
