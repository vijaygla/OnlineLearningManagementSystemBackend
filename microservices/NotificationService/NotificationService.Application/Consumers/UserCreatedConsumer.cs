using MassTransit;
using NotificationService.Application.Interfaces;
using Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace NotificationService.Application.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(IEmailService emailService, ILogger<UserCreatedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("📢 Received UserCreatedEvent for {Email}. OTP Present: {HasOtp}", message.Email, !string.IsNullOrEmpty(message.Otp));

        if (!string.IsNullOrEmpty(message.Otp))
        {
            var subject = "Verify your email - Online Learning Management System";
            var body = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #0ea5e9;'>Welcome to OLMS!</h2>
                    <p>Thank you for registering, <strong>{message.FullName}</strong>.</p>
                    <p>To complete your registration and verify your email address, please use the following 6-digit OTP:</p>
                    <div style='background: #f0f9ff; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0369a1;'>{message.Otp}</span>
                    </div>
                    <p style='color: #64748b; font-size: 14px;'>This OTP is valid for 15 minutes. If you did not request this, please ignore this email.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='color: #94a3b8; font-size: 12px; text-align: center;'>&copy; 2026 Online Learning Management System. All rights reserved.</p>
                </div>";

            await _emailService.SendEmailAsync(message.Email, subject, body);
            _logger.LogInformation("✅ Verification email (with OTP) sent to: {Email}", message.Email);
        }
        else
        {
            // Google users or others without OTP
            var subject = "Welcome to Online Learning Management System";
            var body = $@"
                <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #0ea5e9;'>Welcome to OLMS!</h2>
                    <p>Hello <strong>{message.FullName}</strong>,</p>
                    <p>We're excited to have you on board. Start exploring our courses and level up your skills today!</p>
                    <div style='margin-top: 30px; text-align: center;'>
                        <a href='http://localhost:4200' style='background: #0ea5e9; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold;'>Start Learning</a>
                    </div>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;'>
                    <p style='color: #94a3b8; font-size: 12px; text-align: center;'>&copy; 2026 Online Learning Management System. All rights reserved.</p>
                </div>";

            await _emailService.SendEmailAsync(message.Email, subject, body);
            _logger.LogInformation("✅ Welcome email (no OTP) sent to: {Email}", message.Email);
        }
    }
}
