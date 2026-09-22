using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using AppPlusPlus.Application.Interfaces;

namespace AppPlusPlus.Infrastructure.ExternalServices;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Smtp:Host"])
        && !string.IsNullOrWhiteSpace(_config["Smtp:User"])
        && !string.IsNullOrWhiteSpace(NormalizePassword(_config["Smtp:Password"]));

    public async Task SendAsync(string toAddress, string subject, string bodyHtml,
        byte[]? attachmentBytes = null, string? attachmentName = null, string? replyTo = null)
    {
        var smtp = _config.GetSection("Smtp");
        var host = smtp["Host"]?.Trim()
            ?? throw new InvalidOperationException("SMTP non configuré : Smtp:Host manquant.");
        var port = int.Parse(smtp["Port"] ?? "587");
        var user = smtp["User"]?.Trim()
            ?? throw new InvalidOperationException("SMTP non configuré : Smtp:User manquant.");
        var pass = NormalizePassword(smtp["Password"]);
        if (string.IsNullOrWhiteSpace(pass))
            throw new InvalidOperationException("SMTP non configuré : Smtp:Password manquant (mot de passe d'application Google requis).");

        var from = smtp["From"]?.Trim();
        if (string.IsNullOrWhiteSpace(from))
            from = user;

        var useSsl = bool.Parse(smtp["UseSsl"] ?? "true");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;

        if (!string.IsNullOrWhiteSpace(replyTo) && replyTo.Contains('@', StringComparison.Ordinal))
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo.Trim()));

        var builder = new BodyBuilder { HtmlBody = bodyHtml };

        if (attachmentBytes is not null && attachmentName is not null)
        {
            var contentType = attachmentName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                ? new ContentType("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                : new ContentType("application", "octet-stream");

            builder.Attachments.Add(attachmentName, attachmentBytes, contentType);
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        var sslOption = port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto
        };

        await client.ConnectAsync(host, port, sslOption);

        // Gmail : authentification par mot de passe d'application (pas OAuth).
        if (client.AuthenticationMechanisms.Contains("XOAUTH2"))
            client.AuthenticationMechanisms.Remove("XOAUTH2");

        await client.AuthenticateAsync(user, pass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    static string NormalizePassword(string? password) =>
        (password ?? "").Trim().Replace(" ", "", StringComparison.Ordinal);
}
