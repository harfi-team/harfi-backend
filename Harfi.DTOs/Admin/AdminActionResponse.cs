namespace Harfi.DTOs.Admin;

public class AdminActionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static AdminActionResponse Ok(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static AdminActionResponse Fail(string message) =>
        new() { Success = false, Message = message };
}
