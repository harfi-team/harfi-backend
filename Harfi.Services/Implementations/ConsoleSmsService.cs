using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations;

public class ConsoleSmsService : ISmsService
{
    public Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        Console.WriteLine($"[SMS OTP] To: {phoneNumber} | Code: {otpCode}");
        return Task.FromResult(true);
    }
}
