using Harfi.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Harfi.Services.Implementations;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _config;

    public TwilioSmsService(IConfiguration config)
    {
        _config = config;
    }

    public Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        var accountSid = _config["Sms:AccountSid"];
        var authToken = _config["Sms:AuthToken"];
        var fromNumber = _config["Sms:FromNumber"];

        // TODO: Integrate Twilio SDK to send actual SMS.
        // Message body: $"كود التحقق الخاص بك في حرفي هو: {otpCode} — صالح لمدة 10 دقائق"
        _ = accountSid;
        _ = authToken;
        _ = fromNumber;

        return Task.FromResult(true);
    }
}
