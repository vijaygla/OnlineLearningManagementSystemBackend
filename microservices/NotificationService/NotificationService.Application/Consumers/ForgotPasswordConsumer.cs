using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using Shared.Contracts.Events;

namespace NotificationService.Application.Consumers;

public class ForgotPasswordConsumer : IConsumer<ForgotPasswordEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordConsumer> _logger;

    public ForgotPasswordConsumer(IEmailService emailService, ILogger<ForgotPasswordConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForgotPasswordEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing ForgotPasswordEvent for: {Email}", message.Email);

        var subject = "Reset your password - Online Learning Management System";
        var body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #0ea5e9;'>Password Reset Request</h2>
                <p>Hello <strong>{message.FullName}</strong>,</p>
                <p>We received a request to reset your password. Please use the following 6-digit OTP to set a new password:</p>
                <div style='background: #fdf2f2; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #b91c1c;'>{message.Otp}</span>
                </div>
                <p style='color: #64748b; font-size: 14px;'>This OTP is valid for 15 minutes. If you did not request a password reset, you can safely ignore this email.</p>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='color: #94a3b8; font-size: 12px; text-align: center;'>&copy; 2026 Online Learning Management System. All rights reserved.</p>
            </div>";

        try
        {
            await _emailService.SendEmailAsync(message.Email, subject, body);
            _logger.LogInformation("Password reset email sent to: {Email}", message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to: {Email}", message.Email);
            throw;
        }
    }
}
