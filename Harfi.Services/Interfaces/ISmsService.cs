namespace Harfi.Services.Interfaces;

public interface ISmsService
{
    Task<bool> SendOtpAsync(string phoneNumber, string otpCode);
}
