using MailKit.Net.Smtp;
using MailKit.Security;
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
        
        // Gmail uses Port 587 with STARTTLS
        // If port is 587, use StartTls. If 465, use SslOnConnect. Else None/Auto.
        var secureSocketOptions = _settings.Port == 587 
            ? SecureSocketOptions.StartTls 
            : (_settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

        await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);

        if (!string.IsNullOrEmpty(_settings.Password))
        {
            // Use Username for authentication if provided, otherwise fallback to SenderEmail
            var authUser = !string.IsNullOrEmpty(_settings.Username) ? _settings.Username : _settings.SenderEmail;
            await client.AuthenticateAsync(authUser, _settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
