using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Configurations;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        // For Mailhog or local testing, we often don't need SSL
        await client.ConnectAsync(_settings.Host, _settings.Port, false);

        if (!string.IsNullOrEmpty(_settings.Password))
        {
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
