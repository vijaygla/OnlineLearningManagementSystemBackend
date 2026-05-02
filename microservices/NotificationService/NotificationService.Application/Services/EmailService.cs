using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Configurations;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("📧 Attempting to send email to {To} via {Host}:{Port}", to, _settings.Host, _settings.Port);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();

        try
        {
            var secureSocketOptions = _settings.Port == 587
                ? SecureSocketOptions.StartTls
                : (_settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);

            _logger.LogDebug("Connecting to SMTP server...");
            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);

            if (!string.IsNullOrEmpty(_settings.Password))
            {
                var authUser = !string.IsNullOrEmpty(_settings.Username) ? _settings.Username : _settings.SenderEmail;
                _logger.LogDebug("Authenticating with SMTP server as {User}...", authUser);
                await client.AuthenticateAsync(authUser, _settings.Password);
            }

            await client.SendAsync(message);
            _logger.LogInformation("✅ Email sent successfully to {To}", to);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {To}: {Message}", to, ex.Message);
            throw;
        }
    }}
