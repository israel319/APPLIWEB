namespace AppPlusPlus.Application.Interfaces;

public interface IEmailSender
{
    bool IsConfigured { get; }
    Task SendAsync(string toAddress, string subject, string bodyHtml,
        byte[]? attachmentBytes = null, string? attachmentName = null, string? replyTo = null);
}
