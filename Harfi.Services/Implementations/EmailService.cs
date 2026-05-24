using Harfi.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Harfi.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendVerificationCodeAsync(
        string toEmail, string userName, string code)
    {
        var settings = _config.GetSection("EmailSettings");

        // ── Read & validate config ────────────────────────────
        var host = settings["Host"]
            ?? throw new InvalidOperationException("EmailSettings:Host is missing");
        var port = int.Parse(settings["Port"] ?? "587");
        var senderEmail = settings["SenderEmail"]
            ?? throw new InvalidOperationException("EmailSettings:SenderEmail is missing");
        var appPassword = settings["AppPassword"]
            ?? throw new InvalidOperationException("EmailSettings:AppPassword is missing");
        var senderName = settings["SenderName"] ?? "Harfi Platform";

        // ── Build message ─────────────────────────────────────
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(userName, toEmail));
        message.Subject = "Harfi — Verify your email";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="font-family:Arial;max-width:500px;margin:auto;padding:30px">
                    <h2 style="color:#1a1a2e">مرحباً {userName} 👋</h2>
                    <p>Your verification code is:</p>
                    <div style="font-size:36px;font-weight:bold;
                                letter-spacing:8px;color:#e94560;
                                padding:20px;background:#f5f5f5;
                                text-align:center;border-radius:8px">
                        {code}
                    </div>
                    <p style="color:#888;font-size:13px;margin-top:20px">
                        This code expires in <strong>10 minutes</strong>.<br/>
                        If you didn't register on Harfi, ignore this email.
                    </p>
                </div>
                """
        };

        // ── Send ──────────────────────────────────────────────
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(senderEmail, appPassword);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}